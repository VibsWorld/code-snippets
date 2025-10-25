using System;
using System.Collections.Generic;
using System.Linq;
using ExternalProviders.Zendesk;
using ExternalProviders.Otp.Models;
using System.Diagnostics;
using System.Timers;
using System.Configuration;
using ExternalProviders.Entity.PvOtp;
using System.CodeDom;
using Serilog;
using System.IO;

namespace ExternalProviders.Otp
{
    public static class PVOTP
    {
        
        private static Timer _timer;
        private static Dictionary<string, OtpDetails> _dictOTPData = new Dictionary<string, OtpDetails>();
        
        private static object _lockDictData = new object();        
        private static object _lockRandrom = new object();
        private static object _lockTimer = new object();
        
        
        private static int _defaultOtpLength = 6;
        private static int _defaultOtpTimeOut = 60;
        private static int cleanUpDictonaryInSeconds = 900;

        static Random _random = new Random();

        static ILogger _logger;

        static PVOTP()
        {
            _timer = new Timer();

            _timer.Elapsed += (s, e) =>
            {
                CleanAllExpiredOTPs();
            };

            _timer.Interval = cleanUpDictonaryInSeconds * 1000; //MilliSeconds

            if (ConfigurationManager.AppSettings["DefaultOtpTimeout"] != null)
            {                   
                   if (Int32.TryParse(ConfigurationManager.AppSettings.Get("DefaultOtpTimeout").Trim(), out _defaultOtpTimeOut))
                {
                    Trace.WriteLine($"Default OTP Timeout copied from App Settings named `DefaultOtpTimeout` as {_defaultOtpTimeOut} seconds.");
                }
            }

            if (ConfigurationManager.AppSettings["DefaultOtpLength"] != null)
            {
                if (Int32.TryParse(ConfigurationManager.AppSettings.Get("DefaultOtpLength").Trim(), out _defaultOtpLength))
                {
                    Trace.WriteLine($"Default OTP length copied from App Settings named `DefaultOtpLength` as {_defaultOtpLength} digits. ");
                }
            }

            string folderPath = string.Empty;
            folderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
            var filePath = Path.Combine(folderPath, "PvOtplog.txt");
            _logger = new LoggerConfiguration().WriteTo.File(filePath, rollingInterval: RollingInterval.Day)
                .CreateLogger();
        }       

        public static int TimeOut
        {
            get { return _defaultOtpTimeOut; }
        }

        /// <summary>
        /// This Method Generates an OTP.
        /// </summary>
        /// <param name="inputString">Enter your unique identifer or leave it null to generate one for you</param>
        /// <param name="OTPLength">
        /// Default OTP Length will be changed for this attempt only. To always change default OTP Length please use app settings like <add key="DefaultOtpLength" value="6"/> </param>
        /// <param name="timeOut">
        ///  Please use following app setting as 'DefaultOtpTimeout' (in seconds) to set default timeout always. E.g 
        ///     <add key="DefaultOtpTimeout" value="30"/>
        /// </param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static OtpDetails GenerateOTP(string inputString = null, int? OTPLength = null, int? timeOut = null, ILogger logger = null)
        {
            if (logger == null)
            {
                logger = _logger;
            }
            if (string.IsNullOrWhiteSpace(inputString))
            {
                inputString = Guid.NewGuid().ToString();
                logger.Information("Input String is " + inputString);
            }

            string _salt;
            if (_dictOTPData.Count() > 50 && _timer.Enabled == false)
            {
                lock (_lockTimer)
                {
                    _timer.Start();
                    logger.Information("Timer Stared");
                }                
            }
            else
            {
                lock (_lockTimer)
                {
                    _timer.Stop();
                    logger.Information("Timer Stopped");
                }  
            }

            lock (_lockDictData)
            {
                OtpDetails obj;
                OTPLength = OTPLength.HasValue ? OTPLength.Value : _defaultOtpLength;
                timeOut = timeOut.HasValue ? timeOut.Value : _defaultOtpTimeOut;
                _salt = ZendeskUtils.ConvertToBase64String(inputString);

                if (_dictOTPData.ContainsKey(_salt))
                {
                    logger.Information($"Dictionary Value found with salt {_salt}");
                    obj = _dictOTPData[_salt];
                    if (obj.OtpExpiry.HasValue)
                    {
                        TimeSpan timeDifference = obj.OtpExpiry.Value.Subtract(DateTime.Now);
                        if (timeDifference.TotalSeconds > 1)
                        {
                            logger.Information($"OTP has not yet expired. Timeout Remaining (in seconds is) is {timeDifference.TotalSeconds}. Returning OTP {obj.OTP}");
                            return obj;
                        }
                        else
                        {
                            obj = new OtpDetails
                            {
                                OTP = GenerateOTPNumber(OTPLength.Value),
                                InputIdentifier = inputString,
                                OtpExpiry = DateTime.Now.AddSeconds(timeOut.Value),
                                Salt = _salt
                            };
                            logger.Information($"New OTP generated is " + obj.OTP);
                            _dictOTPData[_salt] = obj;
                            return obj;
                        }
                    }
                    else
                    {
                        throw new Exception("Expiry cannot be null when salt is present in dictionary");
                    }
                }
                else
                {
                    obj = new OtpDetails
                    {
                        InputIdentifier = inputString,
                        Salt = _salt,
                        OTP = GenerateOTPNumber(OTPLength.Value),
                        OtpExpiry = DateTime.Now.AddSeconds(timeOut.Value)
                    };
                    _dictOTPData.Add(_salt, obj);
                    logger.Information($"Dictionary Value not found. Now adding with salt {_salt} with OTP {obj.OTP}");
                    return obj;
                }
            }
        }       

        public static OtpResult VerifyOtp(string salt,string otp, ILogger logger = null)
        {
            logger = logger ?? _logger;

            if (string.IsNullOrWhiteSpace(salt))
            {
                throw new Exception("Salt Entered cannnot be null");
            }
            if (string.IsNullOrWhiteSpace(otp))
            {
                throw new Exception("OTP Cannot be null");
            }
            OtpResult _result = new OtpResult
            {
                Salt = salt,
                UniqueIdentifer = ZendeskUtils.ConvertFromBase64StringToNormalString(salt)
            };

            if (_dictOTPData.ContainsKey(salt))
            {
                OtpDetails obj = _dictOTPData[salt];
                double seconds = obj.OtpExpiry.Value.Subtract(DateTime.Now).TotalSeconds;
                logger.Information($"Verifying salt {salt} with OTP {otp} with remaining {seconds} seconds");
                string _otp = obj.OTP;

                if (_otp != otp)
                {
                    _result.Status = false;
                    _result.Message = "Invalid OTP";
                    logger.Information("returned object {@_result}", _result);
                    return _result;
                }
                else if (seconds > 1 && _otp == otp)
                {
                    _result.Status = true;
                    _result.Message = "OTP is valid";
                    logger.Information("returned object {@_result}", _result);
                    return _result;
                }
                else
                {
                    _result.Status = false;
                    _result.Message = "OTP expired";
                    logger.Information("returned object {@_result}", _result);
                    return _result;
                }
            }
            else
            {                
                _result.Message = "Invalid OTP";
                _result.Status = false;
                logger.Information("returned object {@_result}", _result);
                return _result;
            }
        }

        private static void CleanAllExpiredOTPs(ILogger logger = null)
        {
            logger = logger ?? _logger;
            lock (_lockDictData)
            {
                logger.Information("Running Clean UP Service");
                if (_dictOTPData != null && _dictOTPData.Count > 0)
                {
                    IEnumerable<OtpDetails> obj = _dictOTPData.Values.Where(x => x.OtpExpiry.HasValue == true &&
               x.OtpExpiry.Value.Subtract(DateTime.Now).Seconds < 1);
                    List<string> _listToBeDeletedSalts = new List<string>();
                    foreach (var item in obj)
                    {
                        logger.Information($"Expired salts are {item.InputIdentifier} - {item.OtpExpiry.Value.ToString()}. Deleting now salt {item.Salt} associated with identifier {item.InputIdentifier}");
                        _listToBeDeletedSalts.Add(item.Salt);
                    }

                    if (_listToBeDeletedSalts.Count() > 0)
                    {
                        foreach (string item in _listToBeDeletedSalts)
                        {
                            _dictOTPData.Remove(item);
                        }
                    }
                }

                if (_dictOTPData.Count == 0)
                {
                    lock (_lockTimer)
                    {
                        _timer.Stop();
                        logger.Information("Timer has now stopped");
                    }
                }
            }
        }

        private static string GenerateOTPNumber(int otpLength)
        {
            lock (_lockRandrom)
            {
                int[] intarray = new int[otpLength];
                for (int i = 0; i < intarray.Length; i++)
                {
                    int value = i == 0 ? _random.Next(1, 9) : _random.Next(0, 9);
                    intarray[i] = value;
                }
                return string.Join("", intarray);
            }  
        }
    }
}
