﻿using ReserveBlockCore.Trillium;
using ReserveBlockCore.Utilities;
using System.Text;

namespace ReserveBlockCore.Models.SmartContracts
{
    public class EvolvingFeature
    {
        public int EvolutionState { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsDynamic { get; set; }
        public bool IsCurrentState { get; set; }
        public DateTime? EvolveDate { get; set; }
        public long? EvolveBlockHeight { get; set; }
        public SmartContractAsset? SmartContractAsset { get; set; }

        public static List<EvolvingFeature> GetEvolveFeature(List<string> eList)
        {
            List<EvolvingFeature> evolveFeatures = new List<EvolvingFeature>();

            eList.ForEach(x => {
                EvolvingFeature evolveFeature = new EvolvingFeature();

                var evArray = x.Split(new string[] { "|->" }, StringSplitOptions.None);

                evolveFeature.EvolutionState = Convert.ToInt32(evArray[0].ToString());
                evolveFeature.Name = evArray[1].ToString();
                evolveFeature.Description = evArray[2].ToString();
                //evolveFeature.EvolveDate = 
                if (evArray.Count() == 6)
                {
                    var blockHeight = evArray[5].ToString();
                    if (blockHeight != "")
                    {
                        evolveFeature.EvolveBlockHeight = Convert.ToInt64(evArray[5].ToString());
                    }
                    else
                    {
                        evolveFeature.EvolveBlockHeight = null;
                    }
                }
                else
                {
                    evolveFeature.EvolveBlockHeight = null;
                }    
                

                //Custom vars
                var assetName = evArray[3].ToString();
                if(assetName != "")
                {
                    SmartContractAsset scAsset = new SmartContractAsset { 
                        AssetAuthorName = "",
                        AssetId = Guid.NewGuid(),
                        Extension = "",
                        FileSize = 0,
                        Location = "",
                        Name = assetName,
                    };

                    evolveFeature.SmartContractAsset = scAsset;
                }
                else
                {
                    evolveFeature.SmartContractAsset = null;
                }    
                var evolveDateTicks = evArray[4].ToString();
                if(evolveDateTicks != "")
                {
                    DateTime myDate = new DateTime(Convert.ToInt64(evolveDateTicks));
                    evolveFeature.EvolveDate = myDate;
                }
                else
                {
                    evolveFeature.EvolveDate = null;
                }

                evolveFeatures.Add(evolveFeature);

            });

            return evolveFeatures;
        }

        public static async Task<(bool, string)> GetNewEvolveState(string scText)
        {
            var byteArrayFromBase64 = scText.FromBase64ToByteArray();
            var decompressedByteArray = SmartContractUtility.Decompress(byteArrayFromBase64);
            var textFromByte = Encoding.Unicode.GetString(decompressedByteArray);

            var repl = new TrilliumRepl();
            repl.Run("#reset");
            repl.Run(textFromByte);

            var features = repl.Run(@"GetNFTFeatures()");
            if(features != null)
            {
                var featureList = features.Value.ToString();
                if(featureList != "" && featureList != null)
                {
                    if(featureList.Contains("0"))
                    {
                        var evolveState = repl.Run(@"GetCurrentEvolveState()").Value.ToString();
                        var evolveMaxState = repl.Run(@"EvolveStates()");

                        try
                        {
                            var currentEvoState = evolveState.Replace("{*", "").Replace("}", "");
                            var evolve = repl.Run(@"Evolve(" + currentEvoState + ")").Value.ToString();
                            if(evolve == "Failed to Evolve.")
                            {
                                return (false, "Failed to Evolve NFT");
                            }
                            var newEvolveState = repl.Run(@"GetCurrentEvolveState()").Value.ToString();

                            var newSCData = textFromByte.Replace(evolveState, newEvolveState);

                            return (true, newSCData);
                        }
                        catch(Exception ex)
                        {
                            return (false, "Failed to Evolve NFT");
                        }
                    }
                }
            }
            return (false, "Failed to Evolve NFT");
        }

        public static async Task<(bool, string)> GetNewDevolveState(string scText)
        {
            var byteArrayFromBase64 = scText.FromBase64ToByteArray();
            var decompressedByteArray = SmartContractUtility.Decompress(byteArrayFromBase64);
            var textFromByte = Encoding.Unicode.GetString(decompressedByteArray);

            var repl = new TrilliumRepl();
            repl.Run("#reset");
            repl.Run(textFromByte);

            var features = repl.Run(@"GetNFTFeatures()");
            if (features != null)
            {
                var featureList = features.Value.ToString();
                if (featureList != "" && featureList != null)
                {
                    if (featureList.Contains("0"))
                    {
                        var evolveState = repl.Run(@"GetCurrentEvolveState()").Value.ToString();

                        try
                        {
                            var currentEvoState = evolveState.Replace("{*", "").Replace("}", "");
                            var evolve = repl.Run(@"Devolve(" + currentEvoState + ")").Value.ToString();
                            if (evolve == "Failed to Devolve.")
                            {
                                return (false, "Failed to Devolve NFT");
                            }
                            var newEvolveState = repl.Run(@"GetCurrentEvolveState()").Value.ToString();

                            var newSCData = textFromByte.Replace(evolveState, newEvolveState);

                            return (true, newSCData);
                        }
                        catch (Exception ex)
                        {
                            return (false, "Failed to Devolve NFT");
                        }
                    }
                }
            }
            return (false, "Failed to Devolve NFT");
        }

        public static async Task<(bool, string)> GetNewSpecificState(string scText, int evoState)
        {
            var byteArrayFromBase64 = scText.FromBase64ToByteArray();
            var decompressedByteArray = SmartContractUtility.Decompress(byteArrayFromBase64);
            var textFromByte = Encoding.Unicode.GetString(decompressedByteArray);

            var repl = new TrilliumRepl();
            repl.Run("#reset");
            repl.Run(textFromByte);

            var features = repl.Run(@"GetNFTFeatures()");
            if (features != null)
            {
                var featureList = features.Value.ToString();
                if (featureList != "" && featureList != null)
                {
                    if (featureList.Contains("0"))
                    {
                        var evolveState = repl.Run(@"GetCurrentEvolveState()").Value.ToString();

                        try
                        {
                            var specEvoState = evoState.ToString();
                            var evolve = repl.Run(@"ChangeEvolveStateSpecific(" + specEvoState + ")").Value.ToString();
                            if (evolve == "Failed to Evolve.")
                            {
                                return (false, "Failed to Change State for NFT");
                            }
                            var newEvolveState = repl.Run(@"GetCurrentEvolveState()").Value.ToString();

                            var newSCData = textFromByte.Replace(evolveState, newEvolveState);

                            return (true, newSCData);
                        }
                        catch (Exception ex)
                        {
                            return (false, "Failed to Change State for NFT");
                        }
                    }
                }
            }
            return (false, "Failed to Change State for NFT");
        }
    }
}
