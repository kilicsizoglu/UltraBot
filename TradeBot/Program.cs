using Binance.Net.Clients;
using CryptoExchange.Net.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TradeBot
{
    internal class Program
    {
        static async Task Main(string[] args)
        {

            bool status = false;
            decimal positionPrice = 0;
            decimal positionQuantity = 0;
            string positionSide = "";
            decimal score = 0;

            BinanceRestClient.SetDefaultOptions(options =>
            {
                options.ApiCredentials = new ApiCredentials("APIKEY", "APISECRET"); 
            });

            var client = new BinanceRestClient();

            while (true)
            {
                Thread.Sleep(10000);

                Console.WriteLine("Score :" + score);
                Console.WriteLine("Status :" + status);
                Console.WriteLine("Position Price :" + positionPrice);
                Console.WriteLine("Position Quantity :" + positionQuantity);


                // Volume Degerini al
                var volume = await client.UsdFuturesApi.ExchangeData.GetTakerBuySellVolumeRatioAsync(symbol: "DOGEUSDT",
                    period: Binance.Net.Enums.PeriodInterval.FiveMinutes);

                decimal buyVolume = 0;
                decimal sellVolume = 0;


                var data = volume.Data.Last();

                buyVolume = data.BuyVolume;
                sellVolume = data.SellVolume;

                Console.WriteLine("Buy : " + buyVolume + " Sell : " + sellVolume);

                var price = await client.UsdFuturesApi.ExchangeData.GetPricesAsync();
                decimal latestPrice = 0;
                
                foreach (var item in price.Data)
                {
                    if (item.Symbol == "DOGEUSDT")
                        latestPrice = item.Price;
                }

                if (buyVolume > sellVolume)
                {
                    if (status == false)
                    {
                        // Buy
                        Console.WriteLine("Buy");
                        positionSide = "BUY";
                        positionPrice = latestPrice;
                        positionQuantity = 200 / latestPrice;
                        status = true;
                    }
                    else if (status == true)
                    {
                        // Sell Close
                        if (positionSide == "SELL")
                        {
                            score += (positionQuantity * positionPrice) - (positionQuantity * latestPrice);
                            status = false;
                        }
                    }
                }
                if (buyVolume < sellVolume)
                {
                    if (status == false)
                    {
                        // Sell
                        Console.WriteLine("Sell");
                        positionSide = "SELL";
                        positionPrice = latestPrice;
                        positionQuantity = 200 / latestPrice;
                        status = true;
                    }
                    else if (status == true)
                    {
                        // Buy Close
                        if (positionSide == "BUY")
                        {
                            score += (positionQuantity * latestPrice) - (positionQuantity * positionPrice);
                            status = false;
                        }
                    }
                }

            }

        }
    }
}
