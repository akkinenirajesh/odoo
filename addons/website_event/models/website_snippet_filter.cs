csharp
public partial class WebsiteSnippetFilter {
  // all the model methods are written here.

  public List<object> GetHardcodedSample(object model) {
    List<object> samples = Env.Call<List<object>>("Website.WebsiteSnippetFilter", "GetHardcodedSample", model);
    if (((string)model.GetType().GetProperty("Name").GetValue(model)) == "event.event") {
      List<object> data = new List<object>() {
        new {
          CoverProperties = "{\"background-image\": \"url('/website_event/static/src/img/event_cover_1.jpg')\", \"resize_class\": \"o_record_has_cover o_half_screen_height\", \"opacity\": \"0.4\"}",
          Name = "Great Reno Ballon Race",
          DateBegin = Env.Call<DateTime>("Date", "Today") + new TimeSpan(10, 0, 0, 0),
          DateEnd = Env.Call<DateTime>("Date", "Today") + new TimeSpan(11, 0, 0, 0),
        },
        new {
          CoverProperties = "{\"background-image\": \"url('/website_event/static/src/img/event_cover_2.jpg')\", \"resize_class\": \"o_record_has_cover o_half_screen_height\", \"opacity\": \"0.4\"}",
          Name = "Conference For Architects",
          DateBegin = Env.Call<DateTime>("Date", "Today"),
          DateEnd = Env.Call<DateTime>("Date", "Today") + new TimeSpan(2, 0, 0, 0),
        },
        new {
          CoverProperties = "{\"background-image\": \"url('/website_event/static/src/img/event_cover_3.jpg')\", \"resize_class\": \"o_record_has_cover o_half_screen_height\", \"opacity\": \"0.4\"}",
          Name = "Live Music Festival",
          DateBegin = Env.Call<DateTime>("Date", "Today") + new TimeSpan(8 * 7, 0, 0, 0),
          DateEnd = Env.Call<DateTime>("Date", "Today") + new TimeSpan(8 * 7, 5, 0, 0, 0),
        },
        new {
          CoverProperties = "{\"background-image\": \"url('/website_event/static/src/img/event_cover_5.jpg')\", \"resize_class\": \"o_record_has_cover o_half_screen_height\", \"opacity\": \"0.4\"}",
          Name = "Hockey Tournament",
          DateBegin = Env.Call<DateTime>("Date", "Today") + new TimeSpan(7, 0, 0, 0),
          DateEnd = Env.Call<DateTime>("Date", "Today") + new TimeSpan(7, 0, 0, 0),
        },
        new {
          CoverProperties = "{\"background-image\": \"url('/website_event/static/src/img/event_cover_7.jpg')\", \"resize_class\": \"o_record_has_cover o_half_screen_height\", \"opacity\": \"0.4\"}",
          Name = "OpenWood Collection Online Reveal",
          DateBegin = Env.Call<DateTime>("Date", "Today") + new TimeSpan(1, 0, 0, 0),
          DateEnd = Env.Call<DateTime>("Date", "Today") + new TimeSpan(3, 0, 0, 0),
        },
        new {
          CoverProperties = "{\"background-image\": \"url('/website_event/static/src/img/event_cover_4.jpg')\", \"resize_class\": \"o_record_has_cover o_half_screen_height\", \"opacity\": \"0.4\"}",
          Name = "Business Workshops",
          DateBegin = Env.Call<DateTime>("Date", "Today") + new TimeSpan(2, 0, 0, 0),
          DateEnd = Env.Call<DateTime>("Date", "Today") + new TimeSpan(4, 0, 0, 0),
        }
      };
      List<object> merged = new List<object>();
      int max = Math.Max(samples.Count, data.Count);
      for (int index = 0; index < max; index++) {
        merged.Add(new {
          // merge definitions
          CoverProperties = ((string)samples[index % samples.Count].GetType().GetProperty("CoverProperties").GetValue(samples[index % samples.Count])) + ((string)data[index % data.Count].GetType().GetProperty("CoverProperties").GetValue(data[index % data.Count])),
          Name = (string)samples[index % samples.Count].GetType().GetProperty("Name").GetValue(samples[index % samples.Count]) + (string)data[index % data.Count].GetType().GetProperty("Name").GetValue(data[index % data.Count]),
          DateBegin = (DateTime)samples[index % samples.Count].GetType().GetProperty("DateBegin").GetValue(samples[index % samples.Count]) + (DateTime)data[index % data.Count].GetType().GetProperty("DateBegin").GetValue(data[index % data.Count]),
          DateEnd = (DateTime)samples[index % samples.Count].GetType().GetProperty("DateEnd").GetValue(samples[index % samples.Count]) + (DateTime)data[index % data.Count].GetType().GetProperty("DateEnd").GetValue(data[index % data.Count]),
        });
      }
      samples = merged;
    }
    return samples;
  }
}
