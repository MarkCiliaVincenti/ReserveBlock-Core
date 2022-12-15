﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;
using ReserveBlockCore.Models;
using ReserveBlockCore.P2P;
using ReserveBlockCore.Utilities;

namespace ReserveBlockCore.Controllers
{
    [ActionFilterController]
    [Route("bcapi/[controller]")]
    [Route("bcapi/[controller]/{somePassword?}")]
    [ApiController]
    public class BCV1Controller : ControllerBase
    {
        /// <summary>
        /// Creates a beacon on the local host
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        [HttpGet("CreateBeacon/{name}")]
        public async Task<string> CreateBeacon(string name)
        {
            var output = "";

            var ip = P2PClient.MostLikelyIP();

            if(ip == "NA")
            {
                return "Could not get external IP. Please ensure you are connected to peers and that you are not blocking ports.";
            }

            var bUID = Guid.NewGuid().ToString().Substring(0, 12).Replace("-", "") + ":" + TimeUtil.GetTime().ToString();

            BeaconInfo bInfo = new BeaconInfo();
            bInfo.Name = name;
            bInfo.IsBeaconActive = true;
            bInfo.BeaconUID = bUID;

            BeaconInfo.BeaconInfoJson beaconLoc = new BeaconInfo.BeaconInfoJson
            {
                IPAddress = ip,
                Port = Globals.IsTestNet != true ? Globals.Port + 20000 : Globals.Port + 20000,
                Name = name,
                BeaconUID = bUID
                
            };

            var beaconLocJson = JsonConvert.SerializeObject(beaconLoc);
            bInfo.BeaconLocator = beaconLocJson.ToBase64();

            output = BeaconInfo.SaveBeaconInfo(bInfo);

            return output;
        }
        [HttpGet("DecodeBeaconLocator/{locator}")]
        public async Task<string> DecodeBeaconLocator(string locator)
        {
            var output = "";
            try
            {
                var beaconString = locator.ToStringFromBase64();
                var beaconDataJsonDes = JsonConvert.DeserializeObject<BeaconInfo.BeaconInfoJson>(beaconString);

                output = JsonConvert.SerializeObject(new { Result = "Success", BeaconInfo = beaconDataJsonDes });
            }
            catch(Exception ex)
            {
                output = JsonConvert.SerializeObject(new { Result = "Failed", ResultMessage = "Failed to retrieve beacon info from DB. Possible Corruption."});
            }

            return output;
        }

        [HttpGet("GetBeaconInfo")]
        public async Task<string> GetBeaconInfo()
        {
            var output = "";

            var beaconInfo = BeaconInfo.GetBeaconInfo();
            if(beaconInfo != null)
            {
                BeaconInfo.BeaconInfoJson beaconInfoJsonDes = new BeaconInfo.BeaconInfoJson();
                try
                {
                    var beaconString = beaconInfo.BeaconLocator.ToStringFromBase64();
                    beaconInfoJsonDes = JsonConvert.DeserializeObject<BeaconInfo.BeaconInfoJson>(beaconString);

                    output = JsonConvert.SerializeObject(new { Result = "Success", BeaconInfo = beaconInfo, BeaconLocatorData = beaconInfoJsonDes });
                }
                catch(Exception ex)
                {
                    beaconInfoJsonDes = null;
                    output = JsonConvert.SerializeObject(new { Result = "Failed", ResultMessage = "Failed to retrieve beacon info from DB. Possible Corruption." });
                }
                
            }
            else
            {
                output = JsonConvert.SerializeObject(new { Result = "Failed", ResultMessage = "No Beacon info found." });
            }

            return output;
        }

        [HttpGet("SetBeaconState")]
        public async Task<string> SetBeaconState()
        {
            var output = "";

            var result = BeaconInfo.SetBeaconActiveState();

            if(result == null)
            {
                output = JsonConvert.SerializeObject(new { Result = "Failed", ResultMessage = "Error turning beacon on/off" });
            }
            else
            {
                output = JsonConvert.SerializeObject(new { Result = "Success", ResultMessage = result.Value });
            }

            return output;
        }

        [HttpGet("GetBeaconAssets/{scUID}/{locators}/{**signature}/")]
        public async Task<string> GetBeaconAssets(string scUID, string locators, string signature)
        {
            //signature message = scUID
            string output = "";
            var result = await NFTAssetFileUtility.DownloadAssetFromBeacon(scUID, locators, signature, "NA");
            return output;
        }

        [HttpGet("GetAssetQueue")]
        public async Task<string> GetAssetQueue()
        {
            //signature message = scUID
            string output = "None";

            var aqDB = AssetQueue.GetAssetQueue();
            if(aqDB != null)
            {
                var aqList = aqDB.FindAll().ToList();
                if(aqList.Count() > 0)
                {
                    output = JsonConvert.SerializeObject(aqList);
                }
            }

            return output;
        }

        [HttpGet("GetAssetQuestComplete")]
        public async Task<string> GetAssetQuestComplete()
        {
            //signature message = scUID
            string output = "Done";

            var aqDB = AssetQueue.GetAssetQueue();
            if (aqDB != null)
            {
                var aqList = aqDB.FindAll().ToList();
                if (aqList.Count() > 0)
                {
                    foreach(var item in aqList)
                    {
                        item.IsComplete = true;
                        aqDB.UpdateSafe(item);
                    }
                }
            }

            return output;
        }


        [HttpGet("GetBeaconRequest")]
        public async Task<string> GetBeaconRequest()
        {
            //signature message = scUID
            string output = "None";

            var beaconData = BeaconData.GetBeaconData();
            if (beaconData != null)
            {
                output = JsonConvert.SerializeObject(beaconData);
            }

            return output;
        }
        [HttpGet("GetDeleteBeaconRequest/{id}")]
        public async Task<string> GetDeleteBeaconRequest(int id)
        {
            //signature message = scUID
            string output = "None";

            var result = await BeaconData.DeleteBeaconData(id);

            output = result.ToString();

            return output;
        }

        [HttpGet("GetDeleteBeaconRequestAll")]
        public async Task<string> GetDeleteBeaconRequestAll()
        {
            //signature message = scUID
            string output = "None";

            var result = await BeaconData.DeleteAllBeaconData();

            output = result.ToString();

            return output;
        }
    }
}
