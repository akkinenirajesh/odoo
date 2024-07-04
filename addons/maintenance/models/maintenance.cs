csharp
public partial class MaintenanceStage
{
    // All methods related to MaintenanceStage model should be defined here.
}

public partial class MaintenanceEquipmentCategory
{
    // All methods related to MaintenanceEquipmentCategory model should be defined here.
    public void ComputeFold()
    {
        this.Fold = this.EquipmentCount == 0;
    }

    public void ComputeEquipmentCount()
    {
        // This method should fetch the count of related equipment records
        // and assign it to this.EquipmentCount. 
    }

    public void ComputeMaintenanceCount()
    {
        // This method should fetch the count of related maintenance records
        // and assign it to this.MaintenanceCount and this.MaintenanceOpenCount.
    }
    
    public void OnDelete()
    {
        // This method should check if the category has related equipment
        // or maintenance records. If it does, raise an error message. 
    }
}

public partial class MaintenanceMixin
{
    // All methods related to MaintenanceMixin model should be defined here.
    public void ComputeMaintenanceTeam()
    {
        // This method should check if the record has a MaintenanceTeam assigned
        // and set it to the correct MaintenanceTeam based on the company.
    }

    public void ComputeMaintenanceRequest()
    {
        // This method should calculate the MTTR, LatestFailureDate, 
        // MTBF, and EstimatedNextFailure based on related maintenance records. 
    }

    public void ComputeMaintenanceCount()
    {
        // This method should calculate the total maintenance count and the 
        // open maintenance count for the record.
    }
}

public partial class MaintenanceEquipment
{
    // All methods related to MaintenanceEquipment model should be defined here.
    public void ComputeDisplayName()
    {
        // This method should calculate the display name for the equipment 
        // based on the name and serial number.
    }

    public void OnChangeCategory()
    {
        // This method should set the Technician field based on the 
        // selected category's Technician.
    }

    public void Create()
    {
        // This method should handle the creation of a new equipment record 
        // and subscribe the owner to the record.
    }

    public void Write()
    {
        // This method should handle the updating of an existing equipment 
        // record and subscribe the owner to the record.
    }

    public void ReadGroupCategoryIds()
    {
        // This method should return a list of all categories, even if empty, 
        // for use in the kanban view.
    }
}

public partial class MaintenanceRequest
{
    // All methods related to MaintenanceRequest model should be defined here.
    public void GetDefaultStage()
    {
        // This method should return the default stage for a new 
        // maintenance request.
    }

    public void ComputeMaintenanceTeam()
    {
        // This method should assign the correct MaintenanceTeam to the 
        // request based on the company and equipment.
    }

    public void ComputeTechnician()
    {
        // This method should assign the correct Technician to the 
        // request based on the equipment and category.
    }

    public void ComputeRecurrentMaintenance()
    {
        // This method should check if the maintenance type is preventive 
        // and set the Recurrent field accordingly.
    }

    public void Create()
    {
        // This method should handle the creation of a new maintenance 
        // request and add followers based on the owner and technician. 
    }

    public void Write()
    {
        // This method should handle the updating of an existing maintenance 
        // request. It should also handle the scheduling of recurring 
        // maintenance requests. 
    }

    public void ArchiveEquipmentRequest()
    {
        // This method should archive the maintenance request and disable 
        // recurring maintenance.
    }

    public void ResetEquipmentRequest()
    {
        // This method should reset the maintenance request to the first stage 
        // and activate it.
    }

    public void ReadGroupStageIds()
    {
        // This method should return a list of all stages, even if empty, 
        // for use in the kanban view.
    }

    public void _CheckRepeatInterval()
    {
        // This method should ensure that the RepeatInterval field is 
        // greater than or equal to 1.
    }
}

public partial class MaintenanceTeam
{
    // All methods related to MaintenanceTeam model should be defined here.
    public void ComputeTodoRequests()
    {
        // This method should calculate the number of todo requests, 
        // requests scheduled, high priority requests, blocked requests, 
        // and unscheduled requests for the team.
    }

    public void ComputeEquipment()
    {
        // This method should calculate the number of equipment assigned 
        // to the team.
    }
}
