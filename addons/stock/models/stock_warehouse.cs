C#
public partial class StockWarehouse {
  public StockWarehouse() {
  }

  public void OnChangeCompanyId() {
    var groupUser = Env.Ref("base.group_user");
    var groupStockMultiWarehouses = Env.Ref("stock.group_stock_multi_warehouses");
    var groupStockMultiLocation = Env.Ref("stock.group_stock_multi_locations");
    if (!groupUser.ImpliedIds.Contains(groupStockMultiWarehouses) && !groupUser.ImpliedIds.Contains(groupStockMultiLocation)) {
      Env.Notify(
        title: "Warning",
        message: "Creating a new warehouse will automatically activate the Storage Locations setting"
      );
    }
  }

  public async Task<StockWarehouse> Create(Dictionary<string, object> vals) {
    // create view location for warehouse then create all locations
    var locVals = new Dictionary<string, object> {
      {"Name", vals["Code"]},
      {"Usage", "view"},
      {"Location", Env.Ref("stock.stock_location_locations").Id}
    };
    if (vals.ContainsKey("Company")) {
      locVals["Company"] = vals["Company"];
    }
    vals["ViewLocation"] = (await Env.Create("Stock.Location", locVals)).Id;
    var subLocations = GetLocationsValues(vals);

    foreach (var (fieldName, values) in subLocations) {
      values["Location"] = vals["ViewLocation"];
      if (vals.ContainsKey("Company")) {
        values["Company"] = vals["Company"];
      }
      vals[fieldName] = (await Env.Create("Stock.Location", values)).Id;
    }

    // actually create WH
    var warehouse = await Env.Create("Stock.Warehouse", vals);

    // create sequences and operation types
    var newVals = await CreateOrUpdateSequencesAndPickingTypes();
    await warehouse.Write(newVals);

    // create routes and push/stock rules
    var routeVals = await CreateOrUpdateRoute();
    await warehouse.Write(routeVals);

    // Update global route with specific warehouse rule.
    await CreateOrUpdateGlobalRoutesRules();

    // create route selectable on the product to resupply the warehouse from another one
    await CreateResupplyRoutes(this.ResupplyFrom);

    // update partner data if partner assigned
    if (vals.ContainsKey("Partner")) {
      await UpdatePartnerData(vals["Partner"], vals.ContainsKey("Company") ? vals["Company"] : null);
    }

    // manually update locations' warehouse since it didn't exist at their creation time
    var viewLocation = Env.Get("Stock.Location", vals["ViewLocation"]);
    await (viewLocation | viewLocation.WithContext(activeTest: false).ChildIds).Write({"Warehouse": warehouse.Id});

    CheckMultiwarehouseGroup();

    return warehouse;
  }

  public async Task<List<Dictionary<string, object>>> CopyData(Dictionary<string, object> defaultValues = null) {
    var valsList = await Env.CopyData("Stock.Warehouse", this.Id, defaultValues);
    foreach (var (warehouse, vals) in valsList.Zip(this.Id)) {
      if (!defaultValues.ContainsKey("Name")) {
        vals["Name"] = $"{warehouse["Name"]} (copy)";
      }
      if (!defaultValues.ContainsKey("Code")) {
        vals["Code"] = "COPY";
      }
    }
    return valsList;
  }

  public async Task<bool> Write(Dictionary<string, object> vals) {
    if (vals.ContainsKey("Company")) {
      if (this.Company.Id != vals["Company"]) {
        throw new Exception("Changing the company of this record is forbidden at this point, you should rather archive it and create a new one.");
      }
    }

    var warehouses = this.WithContext(activeTest: false);
    await warehouses.CreateMissingLocations(vals);

    if (vals.ContainsKey("IncomingShipments")) {
      await warehouses.UpdateLocationReception(vals["IncomingShipments"]);
    }
    if (vals.ContainsKey("OutgoingShipments")) {
      await warehouses.UpdateLocationDelivery(vals["OutgoingShipments"]);
    }
    if (vals.ContainsKey("IncomingShipments") || vals.ContainsKey("OutgoingShipments")) {
      await warehouses.UpdateReceptionDeliveryResupply(vals.ContainsKey("IncomingShipments") ? vals["IncomingShipments"] : null, vals.ContainsKey("OutgoingShipments") ? vals["OutgoingShipments"] : null);
    }

    if (vals.ContainsKey("ResupplyFrom") && !vals.ContainsKey("ResupplyRoutes")) {
      var oldResupplyWhs = warehouses.Select(w => new { Id = w.Id, ResupplyWhs = w.ResupplyFrom }).ToDictionary(x => x.Id, x => x.ResupplyWhs);
    }

    // If another partner assigned
    if (vals.ContainsKey("Partner")) {
      if (vals.ContainsKey("Company")) {
        await warehouses.UpdatePartnerData(vals["Partner"], vals["Company"]);
      } else {
        foreach (var warehouse in this.Id) {
          await warehouse.UpdatePartnerData(vals["Partner"], warehouse.Company.Id);
        }
      }
    }

    if (vals.ContainsKey("Code") || vals.ContainsKey("Name")) {
      await warehouses.UpdateNameAndCode(vals.ContainsKey("Name") ? vals["Name"] : null, vals.ContainsKey("Code") ? vals["Code"] : null);
    }

    var res = await Env.Write("Stock.Warehouse", this.Id, vals);

    foreach (var warehouse in warehouses) {
      // check if we need to delete and recreate route
      var depends = GetRoutesValues().SelectMany(v => v.Value.Depends).ToList();
      if (vals.ContainsKey("Code") || depends.Any(d => vals.ContainsKey(d))) {
        var pickingTypeVals = await warehouse.CreateOrUpdateSequencesAndPickingTypes();
        if (pickingTypeVals != null) {
          await warehouse.Write(pickingTypeVals);
        }
      }
      if (depends.Any(d => vals.ContainsKey(d))) {
        var routeVals = await warehouse.CreateOrUpdateRoute();
        if (routeVals != null) {
          await warehouse.Write(routeVals);
        }
      }

      // Check if a global rule(mto, buy, ...) need to be modify.
      // The field that impact those rules are listed in the
      // _get_global_route_rules_values method under the key named
      // 'depends'.
      var globalRules = GetGlobalRouteRulesValues();
      depends = globalRules.SelectMany(v => v.Value.Depends).ToList();
      if (globalRules.Any(r => vals.ContainsKey(r.Key)) || depends.Any(d => vals.ContainsKey(d))) {
        await warehouse.CreateOrUpdateGlobalRoutesRules();
      }

      if (vals.ContainsKey("Active")) {
        var pickingTypeIds = await Env.Search("Stock.PickingType", new Dictionary<string, object> { { "Warehouse", warehouse.Id } }, activeTest: false);
        var moveIds = await Env.Search("Stock.Move", new Dictionary<string, object> {
          { "PickingType", pickingTypeIds.Select(x => x.Id) },
          { "State", new List<string> { "done", "cancel" }, operator: Operator.NotIn }
        });
        if (moveIds.Any()) {
          throw new Exception($"You still have ongoing operations for operation types {string.Join(", ", moveIds.Select(m => m.PickingType.Name))} in warehouse {warehouse.Name}");
        } else {
          await pickingTypeIds.Write(new Dictionary<string, object> { { "Active", vals["Active"] } });
        }
        var locationIds = await Env.Search("Stock.Location", new Dictionary<string, object> { { "Location", warehouse.ViewLocation.Id }, operator: Operator.ChildOf }, activeTest: false);
        var pickingTypeUsingLocations = await Env.Search("Stock.PickingType", new Dictionary<string, object> {
          { "DefaultLocationSource", locationIds.Select(x => x.Id) },
          { "DefaultLocationDestination", locationIds.Select(x => x.Id) },
          { "Id", pickingTypeIds.Select(x => x.Id), operator: Operator.NotIn }
        });
        if (pickingTypeUsingLocations.Any()) {
          throw new Exception($"\"{string.Join(", ", pickingTypeUsingLocations.Select(x => x.Name))}\" have default source or destination locations within warehouse \"{warehouse.Name}\", therefore you cannot archive it.");
        }
        await warehouse.ViewLocation.Write(new Dictionary<string, object> { { "Active", vals["Active"] } });

        var ruleIds = await Env.Search("Stock.Rule", new Dictionary<string, object> { { "Warehouse", warehouse.Id } }, activeTest: false);
        // Only modify route that apply on this warehouse.
        var routes = warehouse.Routes.Where(r => r.WarehouseIds.Count == 1).ToList();
        await routes.Write(new Dictionary<string, object> { { "Active", vals["Active"] } });
        await ruleIds.Write(new Dictionary<string, object> { { "Active", vals["Active"] } });

        if (warehouse.Active) {
          // Catch all warehouse fields that trigger a modfication on
          // routes, rules, picking types and locations (e.g the reception
          // steps). The purpose is to write on it in order to let the
          // write method set the correct field to active or archive.
          var depends = new HashSet<string>();
          depends.UnionWith(GetGlobalRouteRulesValues().SelectMany(v => v.Value.Depends));
          depends.UnionWith(GetRoutesValues().SelectMany(v => v.Value.Depends));
          var values = new Dictionary<string, object> { { "ResupplyRoutes", warehouse.ResupplyRoutes.Select(r => r.Id).ToList() } };
          foreach (var depend in depends) {
            values[depend] = warehouse[depend];
          }
          await warehouse.Write(values);
        }
      }
    }

    if (vals.ContainsKey("ResupplyFrom") && !vals.ContainsKey("ResupplyRoutes")) {
      foreach (var warehouse in warehouses) {
        var newResupplyWhs = warehouse.ResupplyFrom;
        var toAdd = newResupplyWhs.Except(oldResupplyWhs[warehouse.Id]).ToList();
        var toRemove = oldResupplyWhs[warehouse.Id].Except(newResupplyWhs).ToList();
        if (toAdd.Any()) {
          var existingRoutes = await Env.Search("Stock.Route", new Dictionary<string, object> {
            { "SuppliedWh", warehouse.Id },
            { "SupplierWh", toAdd.Select(x => x.Id) },
            { "Active", false }
          });
          if (existingRoutes.Any()) {
            await existingRoutes.ToggleActive();
          }
          var remainingToAdd = toAdd.Except(existingRoutes.SupplierWh).ToList();
          if (remainingToAdd.Any()) {
            await warehouse.CreateResupplyRoutes(remainingToAdd);
          }
        }
        if (toRemove.Any()) {
          var toDisableRouteIds = await Env.Search("Stock.Route", new Dictionary<string, object> {
            { "SuppliedWh", warehouse.Id },
            { "SupplierWh", toRemove.Select(x => x.Id) },
            { "Active", true }
          });
          await toDisableRouteIds.ToggleActive();
        }
      }
    }

    if (vals.ContainsKey("Active")) {
      CheckMultiwarehouseGroup();
    }
    return res;
  }

  public async Task Unlink() {
    await Env.Unlink("Stock.Warehouse", this.Id);
    CheckMultiwarehouseGroup();
  }

  public void CheckMultiwarehouseGroup() {
    var cntByCompany = Env.ReadGroup("Stock.Warehouse", new List<Dictionary<string, object>> { { "Active", true } }, new List<string> { "Company" }, new List<string> { "Count" });
    if (cntByCompany.Any()) {
      var maxCount = cntByCompany.Max(x => (int)x["Count"]);
      var groupUser = Env.Ref("base.group_user");
      var groupStockMultiWarehouses = Env.Ref("stock.group_stock_multi_warehouses");
      var groupStockMultiLocations = Env.Ref("stock.group_stock_multi_locations");
      if (maxCount <= 1 && groupUser.ImpliedIds.Contains(groupStockMultiWarehouses)) {
        groupUser.Write(new Dictionary<string, object> { { "ImpliedIds", new List<object> { new { Command = Command.Remove, Id = groupStockMultiWarehouses.Id } } } });
        groupStockMultiWarehouses.Write(new Dictionary<string, object> { { "Users", new List<object> { new { Command = Command.Remove, Id = groupUser.Users.Select(u => u.Id) } } } });
      }
      if (maxCount > 1 && !groupUser.ImpliedIds.Contains(groupStockMultiWarehouses)) {
        if (!groupUser.ImpliedIds.Contains(groupStockMultiLocations)) {
          Env.Get("Res.Config.Settings").Create(new Dictionary<string, object> { { "GroupStockMultiLocations", true } }).Execute();
        }
        groupUser.Write(new Dictionary<string, object> { { "ImpliedIds", new List<object> {
          new { Command = Command.Add, Id = groupStockMultiWarehouses.Id },
          new { Command = Command.Add, Id = groupStockMultiLocations.Id }
        } } });
      }
    }
  }

  public async Task UpdatePartnerData(object partnerId, object companyId) {
    if (partnerId == null) {
      return;
    }
    var transitLoc = companyId != null ? Env.Get("Res.Company", companyId).InternalTransitLocation.Id : Env.Company.InternalTransitLocation.Id;
    await Env.Get("Res.Partner", partnerId).WithCompany(companyId).Write(new Dictionary<string, object> {
      { "PropertyStockCustomer", transitLoc },
      { "PropertyStockSupplier", transitLoc }
    });
  }

  public async Task<Dictionary<string, object>> CreateOrUpdateSequencesAndPickingTypes() {
    var irSequenceSudo = Env.Get("Ir.Sequence", sudo: true);
    var pickingType = Env.Get("Stock.PickingType");

    // choose the next available color for the operation types of this warehouse
    var allUsedColors = (await pickingType.SearchRead(new Dictionary<string, object> {
      { "Warehouse", new List<object> { new { Command = Command.NotEquals, Id = this.Id } } },
      { "Color", new List<object> { new { Command = Command.NotEquals, Id = null } } }
    }, new List<string> { "Color" }, orderBy: "Color")).Select(x => (int)x["Color"]).ToList();
    var availableColors = Enumerable.Range(0, 12).Except(allUsedColors).ToList();
    var color = availableColors.FirstOrDefault() ?? 0;

    var warehouseData = new Dictionary<string, object>();
    var sequenceData = GetSequenceValues();

    // suit for each warehouse: reception, internal, pick, pack, ship
    var maxSequence = (await pickingType.SearchRead(new Dictionary<string, object> { { "Sequence", new List<object> { new { Command = Command.NotEquals, Id = null } } } }, new List<string> { "Sequence" }, limit: 1, orderBy: "Sequence desc")).FirstOrDefault();
    var maxSequenceValue = maxSequence != null ? (int)maxSequence["Sequence"] : 0;

    var data = GetPickingTypeUpdateValues();
    var (createData, maxSequenceValue) = GetPickingTypeCreateValues(maxSequenceValue);

    foreach (var (pickingTypeField, values) in data) {
      if (this[pickingTypeField] != null) {
        await this[pickingTypeField].Sequence.Write(sequenceData[pickingTypeField]);
        await this[pickingTypeField].Write(values);
      } else {
        data[pickingTypeField] = new Dictionary<string, object>(data[pickingTypeField]);
        data[pickingTypeField].Merge(createData[pickingTypeField]);
        var existingSequence = await irSequenceSudo.SearchCount(new Dictionary<string, object> {
          { "Company", sequenceData[pickingTypeField]["Company"] },
          { "Name", sequenceData[pickingTypeField]["Name"] }
        }, limit: 1);
        var sequence = await irSequenceSudo.Create(sequenceData[pickingTypeField]);
        if (existingSequence > 0) {
          sequence.Name = $"{sequence.Name} (copy)({sequence.Id})";
        }
        data[pickingTypeField].Add("Warehouse", this.Id);
        data[pickingTypeField].Add("Color", color);
        data[pickingTypeField].Add("Sequence", sequence.Id);
        warehouseData[pickingTypeField] = (await pickingType.Create(data[pickingTypeField])).Id;
      }
    }

    if (warehouseData.ContainsKey("OutType")) {
      await pickingType.Get(warehouseData["OutType"]).Write(new Dictionary<string, object> { { "ReturnPickingType", warehouseData.ContainsKey("InType") ? warehouseData["InType"] : null } });
    }
    if (warehouseData.ContainsKey("InType")) {
      await pickingType.Get(warehouseData["InType"]).Write(new Dictionary<string, object> { { "ReturnPickingType", warehouseData.ContainsKey("OutType") ? warehouseData["OutType"] : null } });
    }
    return warehouseData;
  }

  public async Task<bool> CreateOrUpdateGlobalRoutesRules() {
    foreach (var (ruleField, ruleDetails) in GetGlobalRouteRulesValues()) {
      var values = ruleDetails.UpdateValues ?? new Dictionary<string, object>();
      if (this[ruleField] != null) {
        await this[ruleField].Write(values);
      } else {
        values.Merge(ruleDetails.CreateValues);
        values.Add("Warehouse", this.Id);
        this[ruleField] = await Env.Create("Stock.Rule", values);
      }
    }
    return true;
  }

  public async Task<Stock.Route> FindOrCreateGlobalRoute(string xmlId, string routeName, bool create = true, bool raiseIfNotFound = false) {
    // return a route record set from an xml_id or its name. 
    var dataRoute = Env.Ref(xmlId);
    if (dataRoute == null || (dataRoute.Company != null && dataRoute.Company != this.Company)) {
      var route = await Env.Search("Stock.Route", new Dictionary<string, object> {
        { "Name", new List<object> { new { Command = Command.Like, Id = routeName } } },
        { "Company", new List<object> { new { Command = Command.In, Id = new List<object> { null, this.Company.Id } } } }
      }, orderBy: "Company", limit: 1);
      if (route.Any()) {
        return route.First();
      }
      if (raiseIfNotFound) {
        throw new Exception($"Can't find any generic route {routeName}.");
      } else if (dataRoute != null && create) {
        return await dataRoute.Copy(new Dictionary<string, object> {
          { "Name", dataRoute.Name },
          { "Company", this.Company.Id },
          { "RuleIds", new List<object> { new { Command = Command.Remove, Id = dataRoute.RuleIds.Select(x => x.Id) } } }
        });
      }
    }
    return dataRoute;
  }

  public List<KeyValuePair<string, RouteRuleDetails>> GetGlobalRouteRulesValues() {
    // Method used by _create_or_update_global_routes_rules. It's
    // purpose is to return a dict with this format.
    // key: The rule contained in a global route that have to be create/update
    // entry a dict with the following values:
    //   -depends: Field that impact the rule. When a field in depends is
    //   write on the warehouse the rule set as key have to be update.
    //   -create_values: values used in order to create the rule if it does
    //   not exist.
    //   -update_values: values used to update the route when a field in
    //   depends is modify on the warehouse.
    var vals = GenerateGlobalRouteRulesValues();
    // `route_id` might be `False` if the user has deleted it, in such case we
    // should simply ignore the rule
    return vals.Where(v => v.Value.CreateValues.ContainsKey("Route") && v.Value.UpdateValues.ContainsKey("Route")).ToList();
  }

  public List<KeyValuePair<string, RouteRuleDetails>> GenerateGlobalRouteRulesValues() {
    // We use 0 since routing are order from stock to cust. If the routing
    // order is modify, the mto rule will be wrong.
    var rule = GetRulesDict()[this.Id][this.OutgoingShipments].Where(r => r.FromLoc == this.LotStock).First();
    var locationId = rule.FromLoc;
    var locationDestId = rule.DestLoc;
    var pickingTypeId = rule.PickingType;
    return new List<KeyValuePair<string, RouteRuleDetails>> {
      {
        "MtoPull",
        new RouteRuleDetails {
          Depends = new List<string> { "OutgoingShipments" },
          CreateValues = new Dictionary<string, object> {
            { "Active", true },
            { "ProcureMethod", "mts_else_mto" },
            { "Company", this.Company.Id },
            { "Action", "pull" },
            { "Auto", "manual" },
            { "PropagateCarrier", true },
            { "Route", (FindOrCreateGlobalRoute("stock.route_warehouse0_mto", "Replenish on Order (MTO)")).Id }
          },
          UpdateValues = new Dictionary<string, object> {
            { "Name", FormatRuleName(locationId, locationDestId, "MTO") },
            { "LocationDestination", locationDestId.Id },
            { "LocationSource", locationId.Id },
            { "PickingType", pickingTypeId.Id }
          }
        }
      }
    };
  }

  public async Task<Dictionary<string, object>> CreateOrUpdateRoute() {
    // Create routes and active/create their related rules.
    var routes = new List<object>();
    var rulesDict = GetRulesDict();
    foreach (var (routeField, routeData) in GetRoutesValues()) {
      // If the route exists update it
      if (this[routeField] != null) {
        var route = this[routeField];
        if (routeData.RouteUpdateValues != null) {
          await route.Write(routeData.RouteUpdateValues);
        }
        await route.RuleIds.Write(new Dictionary<string, object> { { "Active", false } });
      } else {
        // Create the route
        if (routeData.RouteUpdateValues != null) {
          routeData.RouteCreateValues.Merge(routeData.RouteUpdateValues);
        }
        var route = await Env.Create("Stock.Route", routeData.RouteCreateValues);
        this[routeField] = route;
      }
      // Get rules needed for the route
      var routingKey = routeData.RoutingKey;
      var rules = rulesDict[this.Id][routingKey];
      var rulesValues = new Dictionary<string, object>();
      if (routeData.RulesValues != null) {
        rulesValues.Merge(routeData.RulesValues);
      }
      rulesValues.Add("Route", this[routeField].Id);
      var rulesList = GetRuleValues(rules, rulesValues);
      // Create/Active rules
      await FindExistingRuleOrCreate(rulesList);
      if (routeData.RouteCreateValues.ContainsKey("WarehouseSelectable") && (bool)routeData.RouteCreateValues["WarehouseSelectable"] || routeData.RouteUpdateValues != null && routeData.RouteUpdateValues.ContainsKey("WarehouseSelectable") && (bool)routeData.RouteUpdateValues["WarehouseSelectable"]) {
        routes.Add(this[routeField]);
      }
    }
    return new Dictionary<string, object> { { "Routes", routes.Select(x => new { Command = Command.Add, Id = (int)x }).ToList() } };
  }

  public List<KeyValuePair<string, RouteData>> GetRoutesValues() {
    // Return information in order to update warehouse routes.
    // - The key is a route field sotred as a Many2one on the warehouse
    // - This key contains a dict with route values:
    //   - routing_key: a key used in order to match rules from
    //   get_rules_dict function. It would be usefull in order to generate
    //   the route's rules.
    //   - route_create_values: When the Many2one does not exist the route
    //   is created based on values contained in this dict.
    //   - route_update_values: When a field contained in 'depends' key is
    //   modified and the Many2one exist on the warehouse, the route will be
    //   update with the values contained in this dict.
    //   - rules_values: values added to the routing in order to create the
    //   route's rules.
    return new List<KeyValuePair<string, RouteData>> {
      {
        "ReceiptRoute",
        new RouteData {
          RoutingKey = this.IncomingShipments,
          Depends = new List<string> { "IncomingShipments" },
          RouteUpdateValues = new Dictionary<string, object> {
            { "Name", FormatRouteName(routeType: this.IncomingShipments) },
            { "Active", this.Active }
          },
          RouteCreateValues = new Dictionary<string, object> {
            { "ProductCategSelectable", true },
            { "WarehouseSelectable", true },
            { "ProductSelectable", false },
            { "Company", this.Company.Id },
            { "Sequence", 9 }
          },
          RulesValues = new Dictionary<string, object> {
            { "Active", true },
            { "PropagateCancel", true }
          }
        }
      },
      {
        "DeliveryRoute",
        new RouteData {
          RoutingKey = this.OutgoingShipments,
          Depends = new List<string> { "OutgoingShipments" },
          RouteUpdateValues = new Dictionary<string, object> {
            { "Name", FormatRouteName(routeType: this.OutgoingShipments) },
            { "Active", this.Active }
          },
          RouteCreateValues = new Dictionary<string, object> {
            { "ProductCategSelectable", true },
            { "WarehouseSelectable", true },
            { "ProductSelectable", false },
            { "Company", this.Company.Id },
            { "Sequence", 10 }
          },
          RulesValues = new Dictionary<string, object> {
            { "Active", true },
            { "PropagateCarrier", true }
          }
        }
      },
      {
        "CrossdockRoute",
        new RouteData {
          RoutingKey = "crossdock",
          Depends = new List<string> { "OutgoingShipments", "IncomingShipments" },
          RouteUpdateValues = new Dictionary<string, object> {
            { "Name", FormatRouteName(routeType: "crossdock") },
            { "Active", this.IncomingShipments != "OneStep" && this.OutgoingShipments != "ShipOnly" }
          },
          RouteCreateValues = new Dictionary<string, object> {
            { "ProductSelectable", true },
            { "ProductCategSelectable", true },
            { "Active", this.OutgoingShipments != "ShipOnly" && this.IncomingShipments != "OneStep" },
            { "Company", this.Company.Id },
            { "Sequence", 20 }
          },
          RulesValues = new Dictionary<string, object> {
            { "Active", true },
            { "ProcureMethod", "make_to_order" }
          }
        }
      }
    };
  }

  public List<KeyValuePair<string, RouteData>> GetReceiveRoutesValues(string installedDepends) {
    // Return receive route values with 'procure_method': 'make_to_order'
    // in order to update warehouse routes.

    // This function has the same receive route values as _get_routes_values with the addition of
    // 'procure_method': 'make_to_order' to the 'rules_values'. This is expected to be used by
    // modules that extend stock and add actions that can trigger receive 'make_to_order' rules (i.e.
    // we don't want any of the generated rules by get_rules_dict to default to 'make_to_stock').
    // Additionally this is expected to be used in conjunction with _get_receive_rules_dict().

    // args:
    // installed_depends - string value of installed (warehouse) boolean to trigger updating of reception route.
    return new List<KeyValuePair<string, RouteData>> {
      {
        "ReceiptRoute",
        new RouteData {
          RoutingKey = this.IncomingShipments,
          Depends = new List<string> { "IncomingShipments", installedDepends },
          RouteUpdateValues = new Dictionary<string, object> {
            { "Name", FormatRouteName(routeType: this.IncomingShipments) },
            { "Active", this.Active }
          },
          RouteCreateValues = new Dictionary<string, object> {
            { "ProductCategSelectable", true },
            { "WarehouseSelectable", true },
            { "ProductSelectable", false },
            { "Company", this.Company.Id },
            { "Sequence", 9 }
          },
          RulesValues = new Dictionary<string, object> {
            { "Active", true },
            { "PropagateCancel", true },
            { "ProcureMethod", "make_to_order" }
          }
        }
      }
    };
  }

  public async Task FindExistingRuleOrCreate(List<Dictionary<string, object>> rulesList) {
    // This