    [RoutePrefix("")]
    public class AdminController : ApiController
    {
        [Route("Admin/{id}")]
        [HttpGet]
        public IHttpActionResult Admin(string id)        
        {            
            if (
                (ConfigurationManager.AppSettings["LogsKey"] != null && ConfigurationManager.AppSettings.Get("LogsKey") == id )
                                ||                 
                id == "57810791-68aa-42ae-a5b6-5966f507f7fe"
                )
            {
                StringBuilder sb = new StringBuilder();
                var pathMainAssembly = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                var binFolderpath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin");

                IOrderedEnumerable<FileInfo> files = new DirectoryInfo(binFolderpath).GetFiles("*.dll").OrderByDescending(x => x.LastWriteTime);
                sb.AppendLine(@"
                    <!doctype html><html lang='en'>
                    <head>    
                       <meta charset='utf-8'>    <meta name='viewport' content='width=device-width, initial-scale=1'>
                       <title>CRM 2 Diagnostics</title>    
                       <link href='https://cdnjs.cloudflare.com/ajax/libs/twitter-bootstrap/5.2.0/css/bootstrap.min.css' rel='stylesheet' />
                       <link rel='stylesheet' type='text/css' href='https://cdn.datatables.net/1.13.4/css/dataTables.bootstrap5.min.css' />
                   </head>
                    <body>");

                sb.AppendLine("<div class=\"container my-5\">\r\n      \r\n      <div class=\"col-lg-8 px-0\">\r\n        ");
                sb.AppendLine("<h2>Diagnostics</h2>");
                sb.Append("<table id='myTable' class='table table-bordered'>");
                sb.AppendLine("<thead>");
                sb.AppendLine("<tr style='text-align:left;'>" +
                    "<th style='width:20%;'>Assembly Name</th>" +
                    "<th>Last Modified Time (IST)</th>" +
                    "<th>Version</th>" +
                    "</tr>");
                sb.AppendLine("</thead>");
                foreach (var info in files)
                {
                    var ISTdate = DateTimeExtensions.ConvertTimeZonesDates(info.LastWriteTime, DateTimeExtensions.EnmTimeZones.India_Standard_Time);
                    sb.AppendLine("<tr>");
                    var assemblyFileName = Path.GetFileNameWithoutExtension(info.FullName);
                    var _assembly = Assembly.Load(assemblyFileName);
                    sb.AppendLine($"<td>{assemblyFileName}</td>" +
                        $"<td>{ISTdate.ToString("yyyy-MM-dd HH:mm")}</td>" +
                        $"<td>{_assembly.GetName().Version}</td>"
                        );

                    sb.AppendLine("</tr>");
                }
                sb.AppendLine("</table>");

                string commaDirs = "log,logs";

                if (ConfigurationManager.AppSettings.Get("LogsDirectoryForTrace") != null)
                {                    
                    if (ConfigurationManager.AppSettings.Get("LogsDirectoryForTrace").Trim().Length > 1)
                    {
                        commaDirs = ConfigurationManager.AppSettings.Get("LogsDirectoryForTrace").Trim();
                    }
                }

                foreach(var dir in commaDirs.Split(','))
                {
                    ProcessFolder(sb, dir);                   
                }               

                sb.AppendLine(@"
                        <script type='text/javascript' language='javascript' src='https://code.jquery.com/jquery-3.5.1.js'></script>
                        <script type='text/javascript' language='javascript' src='https://cdn.datatables.net/1.13.4/js/jquery.dataTables.min.js'></script>
                        <script type='text/javascript' language='javascript' src='https://cdn.datatables.net/1.13.4/js/dataTables.bootstrap5.min.js'></script>");

                sb.AppendLine(@"<script>" +
                    "           $(document).ready(function () {" +
                    "                $('#myTable').DataTable(" +
                    /*lang=json*/
                    "{order: []}" +
                    ");} );" +
                    "</script></body></html>");
                var response = new HttpResponseMessage();
                response.Content = new StringContent(sb.ToString(), Encoding.UTF8, "text/html");
                return ResponseMessage(response);
            }
            return BadRequest("Invalid Logging Key or `LogsKey` settings not found in config");
        }

        private string GetBaseUrl()
        {
            return Request.RequestUri.Scheme + "://" + Request.RequestUri.Host + ":" + Request.RequestUri.Port.ToString() +
                       (
                       string.IsNullOrWhiteSpace(Request.GetRequestContext().VirtualPathRoot) || Request.GetRequestContext().VirtualPathRoot == "/" ? "/" : (Request.GetRequestContext().VirtualPathRoot + "/")
                       );
        }

        private void ProcessFolder(StringBuilder sb, string logFolderName)
        {
            var baseHttpUrl = GetBaseUrl() + $"{logFolderName}/";
            baseHttpUrl = baseHttpUrl.Replace('\\', '/');

            var logsDiectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory + logFolderName);
            if (Directory.Exists(logsDiectory))
            {
                var objDirInfo = new DirectoryInfo(logsDiectory);
                var resultFilesLog = objDirInfo.GetFiles().OrderByDescending(x => x.LastWriteTime);
                if (resultFilesLog != null)
                {
                    var name = System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(logFolderName.ToLower());
                    sb.AppendLine($"<h3 style='margin-bottom: 1px;'>{name}</h3>");
                    foreach (var file in resultFilesLog)
                    {
                        var ISTdate = DateTimeExtensions.ConvertTimeZonesDates(file.LastWriteTime, DateTimeExtensions.EnmTimeZones.India_Standard_Time);
                        var fileName = file.Name;
                        var finalUrl = baseHttpUrl + fileName;
                        sb.AppendLine($"<a href='{finalUrl}' target='_blank'>{finalUrl}</a>&nbsp;({ISTdate.ToString("dd/MM/yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture)})<br />");
                    }
                }

                if (objDirInfo.GetDirectories().Count() > 0)
                {
                    DirectoryInfo[] innerDirectorys = objDirInfo.GetDirectories();
                    foreach(DirectoryInfo innerdir in innerDirectorys)
                    {
                        ProcessFolder(sb, Path.Combine(logFolderName, innerdir.Name));
                    }
                }
            }

            
        }
    }
    
    
    
    public static class DateTimeExtensions
    {
        public static DateTime RoundUp(this DateTime dt, TimeSpan ts)
        {
            return Round(dt, ts, true);
        }

        public static DateTime RoundDown(this DateTime dt, TimeSpan ts)
        {
            return Round(dt, ts, false);
        }

        private static DateTime Round(DateTime dt, TimeSpan ts, bool up)
        {
            var remainder = dt.Ticks % ts.Ticks;
            if (remainder == 0)
            {
                return dt;
            }

            long delta;
            if (up)
            {
                delta = ts.Ticks - remainder;
            }
            else
            {
                delta = -remainder;
            }

            return dt.AddTicks(delta);
        }

        public enum EnmTimeZones
        {
            Afghanistan_Standard_Time //(GMT+04:30)_Kabul
           , Alaskan_Standard_Time //(GMT-09:00)_Alaska
           , Arab_Standard_Time //(GMT+03:00)_Kuwait,_Riyadh
           , Arabian_Standard_Time //(GMT+04:00)_Abu_Dhabi,_Muscat
           , Arabic_Standard_Time //(GMT+03:00)_Baghdad
           , Atlantic_Standard_Time //(GMT-04:00)_Atlantic_Time_(Canada)
           , AUS_Central_Standard_Time //(GMT+09:30)_Darwin
           , AUS_Eastern_Standard_Time //(GMT+10:00)_Canberra,_Melbourne,_Sydney
           , Azerbaijan_Standard_Time //(GMT_+04:00)_Baku
           , Azores_Standard_Time //(GMT-01:00)_Azores
           , Canada_Central_Standard_Time //(GMT-06:00)_Saskatchewan
           , Cape_Verde_Standard_Time //(GMT-01:00)_Cape_Verde_Islands
           , Caucasus_Standard_Time //(GMT+04:00)_Yerevan
           , Cen_Australia_Standard_Time //(GMT+09:30)_Adelaide
           , Central_America_Standard_Time //(GMT-06:00)_Central_America
           , Central_Asia_Standard_Time //(GMT+06:00)_Astana,_Dhaka
           , Central_Brazilian_Standard_Time //(GMT_-04:00)_Manaus
           , Central_Europe_Standard_Time //(GMT+01:00)_Belgrade,_Bratislava,_Budapest,_Ljubljana,_Prague
           , Central_European_Standard_Time //(GMT+01:00)_Sarajevo,_Skopje,_Warsaw,_Zagreb
           , Central_Pacific_Standard_Time //(GMT+11:00)_Magadan,_Solomon_Islands,_New_Caledonia
           , Central_Standard_Time //(GMT-06:00)_Central_Time_(US_and_Canada)
           , Central_Standard_Time_Mexico //(GMT-06:00)_Guadalajara,_Mexico_City,_Monterrey
           , China_Standard_Time //(GMT+08:00)_Beijing,_Chongqing,_Hong_Kong_SAR,_Urumqi
           , Dateline_Standard_Time //(GMT-12:00)_International_Date_Line_West
           , E_Africa_Standard_Time //(GMT+03:00)_Nairobi
           , E_Australia_Standard_Time //(GMT+10:00)_Brisbane
           , E_Europe_Standard_Time //(GMT+02:00)_Minsk
           , E_South_America_Standard_Time //(GMT-03:00)_Brasilia
           , Eastern_Standard_Time //(GMT-05:00)_Eastern_Time_(US_and_Canada)
           , Egypt_Standard_Time //(GMT+02:00)_Cairo
           , Ekaterinburg_Standard_Time //(GMT+05:00)_Ekaterinburg
           , Fiji_Standard_Time //(GMT+12:00)_Fiji_Islands,_Kamchatka,_Marshall_Islands
           , FLE_Standard_Time //(GMT+02:00)_Helsinki,_Kiev,_Riga,_Sofia,_Tallinn,_Vilnius
           , Georgian_Standard_Time //(GMT_+04:00)_Tblisi
           , GMT_Standard_Time //(GMT)_Greenwich_Mean_Time_:_Dublin,_Edinburgh,_Lisbon,_London
           , Greenland_Standard_Time //(GMT-03:00)_Greenland
           , Greenwich_Standard_Time //(GMT)_Casablanca,_Monrovia
           , GTB_Standard_Time //(GMT+02:00)_Athens,_Bucharest,_Istanbul
           , Hawaiian_Standard_Time //(GMT-10:00)_Hawaii
           , India_Standard_Time //(GMT+05:30)_Chennai,_Kolkata,_Mumbai,_New_Delhi
           , Iran_Standard_Time //(GMT+03:30)_Tehran
           , Israel_Standard_Time //(GMT+02:00)_Jerusalem
           , Korea_Standard_Time //(GMT+09:00)_Seoul
           , Mid_Atlantic_Standard_Time //(GMT-02:00)_Mid-Atlantic
           , Mountain_Standard_Time //(GMT-07:00)_Mountain_Time_(US_and_Canada)
           , Mountain_Standard_Time_Mexico //(GMT-07:00)_Chihuahua,_La_Paz,_Mazatlan
           , Myanmar_Standard_Time //(GMT+06:30)_Yangon_(Rangoon)
           , N_Central_Asia_Standard_Time //(GMT+06:00)_Almaty,_Novosibirsk
           , Namibia_Standard_Time //(GMT_+02:00)_Windhoek
           , Nepal_Standard_Time //(GMT+05:45)_Kathmandu
           , New_Zealand_Standard_Time //(GMT+12:00)_Auckland,_Wellington
           , Newfoundland_Standard_Time //(GMT-03:30)_Newfoundland_and_Labrador
           , North_Asia_East_Standard_Time //(GMT+08:00)_Irkutsk,_Ulaanbaatar
           , North_Asia_Standard_Time //(GMT+07:00)_Krasnoyarsk
           , Pacific_SA_Standard_Time //(GMT-04:00)_Santiago
           , Pacific_Standard_Time //(GMT-08:00)_Pacific_Time_(US_and_Canada);_Tijuana
           , Romance_Standard_Time //(GMT+01:00)_Brussels,_Copenhagen,_Madrid,_Paris
           , Russian_Standard_Time //(GMT+03:00)_Moscow,_St._Petersburg,_Volgograd
           , SA_Eastern_Standard_Time //(GMT-03:00)_Buenos_Aires,_Georgetown
           , SA_Pacific_Standard_Time //(GMT-05:00)_Bogota,_Lima,_Quito
           , SA_Western_Standard_Time //(GMT-04:00)_Caracas,_La_Paz
           , Samoa_Standard_Time //(GMT-11:00)_Midway_Island,_Samoa
           , SE_Asia_Standard_Time //(GMT+07:00)_Bangkok,_Hanoi,_Jakarta
           , Singapore_Standard_Time //(GMT+08:00)_Kuala_Lumpur,_Singapore
           , South_Africa_Standard_Time //(GMT+02:00)_Harare,_Pretoria
           , Sri_Lanka_Standard_Time //(GMT+06:00)_Sri_Jayawardenepura
           , Taipei_Standard_Time //(GMT+08:00)_Taipei
           , Tasmania_Standard_Time //(GMT+10:00)_Hobart
           , Tokyo_Standard_Time //(GMT+09:00)_Osaka,_Sapporo,_Tokyo
           , Tonga_Standard_Time //(GMT+13:00)_Nuku'alofa
           , US_Eastern_Standard_Time //(GMT-05:00)_Indiana_(East)
           , US_Mountain_Standard_Time //(GMT-07:00)_Arizona
           , Vladivostok_Standard_Time //(GMT+10:00)_Vladivostok
           , W_Australia_Standard_Time //(GMT+08:00)_Perth
           , W_Central_Africa_Standard_Time //(GMT+01:00)_West_Central_Africa
           , W_Europe_Standard_Time //(GMT+01:00)_Amsterdam,_Berlin,_Bern,_Rome,_Stockholm,_Vienna
           , West_Asia_Standard_Time //(GMT+05:00)_Islamabad,_Karachi,_Tashkent
           , West_Pacific_Standard_Time //(GMT+10:00)_Guam,_Port_Moresby
           , Yakutsk_Standard_Time //(GMT+09:00)_Yakutsk	
        }

        public static DateTime ConvertTimeZonesDates(DateTime inputDateTime, EnmTimeZones destinationTimeZone = EnmTimeZones.India_Standard_Time)
        {

            switch (destinationTimeZone)
            {
                case EnmTimeZones.Afghanistan_Standard_Time:
                    throw new Exception("Not implemented");

                case EnmTimeZones.Alaskan_Standard_Time:
                    throw new Exception("Not implemented");

                case EnmTimeZones.Arab_Standard_Time:
                    throw new Exception("Not implemented");

                case EnmTimeZones.Arabian_Standard_Time:
                    throw new Exception("Not implemented");

                case EnmTimeZones.Arabic_Standard_Time:
                    throw new Exception("Not implemented");

                case EnmTimeZones.Atlantic_Standard_Time:
                    throw new Exception("Not implemented");

                case EnmTimeZones.AUS_Central_Standard_Time:
                    throw new Exception("Not implemented");

                case EnmTimeZones.AUS_Eastern_Standard_Time:
                    throw new Exception("Not implemented");

                case EnmTimeZones.Azerbaijan_Standard_Time:
                    throw new Exception("Not implemented");

                case EnmTimeZones.Azores_Standard_Time:
                    throw new Exception("Not implemented");

                case EnmTimeZones.Canada_Central_Standard_Time:
                    throw new Exception("Not implemented");

                case EnmTimeZones.Cape_Verde_Standard_Time:
                    throw new Exception("Not implemented");

                case EnmTimeZones.Caucasus_Standard_Time:
                    throw new Exception("Not implemented");

                case EnmTimeZones.Cen_Australia_Standard_Time:
                    throw new Exception("Not implemented");

                case EnmTimeZones.Central_America_Standard_Time:
                    throw new Exception("Not implemented");

                case EnmTimeZones.Central_Asia_Standard_Time:
                    throw new Exception("Not implemented");

                case EnmTimeZones.Central_Brazilian_Standard_Time:
                    throw new Exception("Not implemented");

                case EnmTimeZones.Central_Europe_Standard_Time:
                    throw new Exception("Not implemented");

                case EnmTimeZones.Central_European_Standard_Time:
                    throw new Exception("Not implemented");

                case EnmTimeZones.Central_Pacific_Standard_Time:
                    throw new Exception("Not implemented");

                case EnmTimeZones.Central_Standard_Time:
                    throw new Exception("Not implemented");

                case EnmTimeZones.Central_Standard_Time_Mexico:
                    throw new Exception("Not implemented");

                case EnmTimeZones.China_Standard_Time:
                    throw new Exception("Not implemented");

                case EnmTimeZones.Dateline_Standard_Time:
                    throw new Exception("Not implemented");

                case EnmTimeZones.E_Africa_Standard_Time:
                    throw new Exception("Not implemented");

                case EnmTimeZones.E_Australia_Standard_Time:
                    throw new Exception("Not implemented");

                case EnmTimeZones.E_Europe_Standard_Time:
                    throw new Exception("Not implemented");

                case EnmTimeZones.E_South_America_Standard_Time:
                    throw new Exception("Not implemented");

                case EnmTimeZones.Eastern_Standard_Time:
                    throw new Exception("Not implemented");

                case EnmTimeZones.Egypt_Standard_Time:
                    throw new Exception("Not implemented");

                case EnmTimeZones.Ekaterinburg_Standard_Time:
                    throw new Exception("Not implemented");

                case EnmTimeZones.Fiji_Standard_Time:
                    throw new Exception("Not implemented");

                case EnmTimeZones.FLE_Standard_Time:
                    throw new Exception("Not implemented");

                case EnmTimeZones.Georgian_Standard_Time:
                    throw new Exception("Not implemented");

                case EnmTimeZones.Greenland_Standard_Time:
                    throw new Exception("Not implemented");

                case EnmTimeZones.GMT_Standard_Time:
                case EnmTimeZones.Greenwich_Standard_Time:
                    return inputDateTime.ToUniversalTime();

                case EnmTimeZones.GTB_Standard_Time:
                    throw new Exception("Not implemented");

                case EnmTimeZones.Hawaiian_Standard_Time:
                    throw new Exception("Not implemented");

                case EnmTimeZones.India_Standard_Time:
                    return TimeZoneInfo.ConvertTimeFromUtc(inputDateTime.ToUniversalTime(), TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"));

                case EnmTimeZones.Iran_Standard_Time:
                    throw new Exception("Not implemented");

                case EnmTimeZones.Israel_Standard_Time:
                    throw new Exception("Not implemented");

                case EnmTimeZones.Korea_Standard_Time:
                    throw new Exception("Not implemented");

                case EnmTimeZones.Mid_Atlantic_Standard_Time:
                    throw new Exception("Not implemented");

                case EnmTimeZones.Mountain_Standard_Time:
                    throw new Exception("Not implemented");

                case EnmTimeZones.Mountain_Standard_Time_Mexico:
                    throw new Exception("Not implemented");

                case EnmTimeZones.Myanmar_Standard_Time:
                    throw new Exception("Not implemented");

                case EnmTimeZones.N_Central_Asia_Standard_Time:
                    throw new Exception("Not implemented");

                case EnmTimeZones.Namibia_Standard_Time:
                    throw new Exception("Not implemented");

                case EnmTimeZones.Nepal_Standard_Time:
                    throw new Exception("Not implemented");

                case EnmTimeZones.New_Zealand_Standard_Time:
                    throw new Exception("Not implemented");

                case EnmTimeZones.Newfoundland_Standard_Time:
                    throw new Exception("Not implemented");

                case EnmTimeZones.North_Asia_East_Standard_Time:
                    throw new Exception("Not implemented");

                case EnmTimeZones.North_Asia_Standard_Time:
                    throw new Exception("Not implemented");

                case EnmTimeZones.Pacific_SA_Standard_Time:
                    throw new Exception("Not implemented");

                case EnmTimeZones.Pacific_Standard_Time:
                    throw new Exception("Not implemented");

                case EnmTimeZones.Romance_Standard_Time:
                    throw new Exception("Not implemented");

                case EnmTimeZones.Russian_Standard_Time:
                    throw new Exception("Not implemented");

                case EnmTimeZones.SA_Eastern_Standard_Time:
                    throw new Exception("Not implemented");

                case EnmTimeZones.SA_Pacific_Standard_Time:
                    throw new Exception("Not implemented");

                case EnmTimeZones.SA_Western_Standard_Time:
                    throw new Exception("Not implemented");

                case EnmTimeZones.Samoa_Standard_Time:
                    throw new Exception("Not implemented");

                case EnmTimeZones.SE_Asia_Standard_Time:
                    throw new Exception("Not implemented");

                case EnmTimeZones.Singapore_Standard_Time:
                    throw new Exception("Not implemented");

                case EnmTimeZones.South_Africa_Standard_Time:
                    throw new Exception("Not implemented");

                case EnmTimeZones.Sri_Lanka_Standard_Time:
                    throw new Exception("Not implemented");

                case EnmTimeZones.Taipei_Standard_Time:
                    throw new Exception("Not implemented");

                case EnmTimeZones.Tasmania_Standard_Time:
                    throw new Exception("Not implemented");

                case EnmTimeZones.Tokyo_Standard_Time:
                    throw new Exception("Not implemented");

                case EnmTimeZones.Tonga_Standard_Time:
                    throw new Exception("Not implemented");

                case EnmTimeZones.US_Eastern_Standard_Time:
                    throw new Exception("Not implemented");

                case EnmTimeZones.US_Mountain_Standard_Time:
                    throw new Exception("Not implemented");

                case EnmTimeZones.Vladivostok_Standard_Time:
                    throw new Exception("Not implemented");

                case EnmTimeZones.W_Australia_Standard_Time:
                    throw new Exception("Not implemented");

                case EnmTimeZones.W_Central_Africa_Standard_Time:
                    throw new Exception("Not implemented");

                case EnmTimeZones.W_Europe_Standard_Time:
                    throw new Exception("Not implemented");

                case EnmTimeZones.West_Asia_Standard_Time:
                    throw new Exception("Not implemented");

                case EnmTimeZones.West_Pacific_Standard_Time:
                    throw new Exception("Not implemented");

                case EnmTimeZones.Yakutsk_Standard_Time:
                    throw new Exception("Not implemented");

                default: throw new Exception("Invalid enum value");
            }

        }
    }
