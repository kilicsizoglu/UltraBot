using Binance.Net.Clients;
using Binance.Net.Enums;
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
            bool locked = false;
            bool status = false;
            decimal positionPrice = 0;
            decimal positionQuantity = 0;
            string positionSide = "";
            decimal score = 0;
            decimal oldBuyVolume = 0;
            decimal oldSellVolume = 0;

            BinanceRestClient.SetDefaultOptions(options =>
            {
                options.ApiCredentials = new ApiCredentials("APIKEY", "APISECRET"); 
            });

            var client = new BinanceRestClient();

            while (true)
            {
                // Change Leverage
                await client.UsdFuturesApi.Account.ChangeInitialLeverageAsync(symbol: "DOGEUSDT", leverage: 25);

                await Task.Delay(10000);

                Console.WriteLine("Score :" + score);
                Console.WriteLine("Status :" + status);
                Console.WriteLine("Position Price :" + positionPrice);
                Console.WriteLine("Position Quantity :" + positionQuantity);
                Console.WriteLine("Position Side :" + positionSide);
             

                // Volume Degerini al
                var volume = await client.UsdFuturesApi.ExchangeData.GetTakerBuySellVolumeRatioAsync(symbol: "DOGEUSDT",
                    period: Binance.Net.Enums.PeriodInterval.FiveMinutes);

                if (volume == null)
                    continue;

                decimal buyVolume = 0;
                decimal sellVolume = 0;


                var data = volume.Data.Last();

                buyVolume = data.BuyVolume;
                sellVolume = data.SellVolume;

                Console.WriteLine("Buy : " + buyVolume + " Sell : " + sellVolume);

                var price = await client.UsdFuturesApi.ExchangeData.GetPricesAsync();
                if (price == null)
                    continue;
                decimal latestPrice = 0;
                
                foreach (var item in price.Data)
                {
                    if (item.Symbol == "DOGEUSDT")
                        latestPrice = item.Price;
                }

                if (oldSellVolume < sellVolume && positionSide == "SELL")
                {
                    locked = true;
                }
                else if (oldBuyVolume > buyVolume && positionSide == "BUY")
                {
                    locked = true;
                }
                else
                {
                    locked = false;
                }

                if (buyVolume > sellVolume && locked == false)
                {
                    if (status == false)
                    {
                        // Buy
                        Console.WriteLine("Buy");
                        positionSide = "BUY";
                        positionPrice = latestPrice;
                        positionQuantity = 200 / latestPrice;
                        status = true;
                        await client.UsdFuturesApi.Trading.PlaceOrderAsync(symbol: "DOGEUSDT", side: OrderSide.Buy, type: FuturesOrderType.Market, quantity: positionQuantity * 25);
                    }
                    else if (status == true)
                    {
                        // Sell Close
                        if (positionSide == "SELL")
                        {
                            score += (positionQuantity * positionPrice) - (positionQuantity * latestPrice);
                            status = false;
                            await client.UsdFuturesApi.Trading.PlaceOrderAsync(symbol: "DOGEUSDT", side: OrderSide.Sell, type: FuturesOrderType.Market, quantity: positionQuantity * 25);
                        }
                    }
                }
                if (oldSellVolume < sellVolume && positionSide == "SELL")
                {
                    score += (positionQuantity * positionPrice) - (positionQuantity * latestPrice);
                    status = false;
                    await client.UsdFuturesApi.Trading.PlaceOrderAsync(symbol: "DOGEUSDT", side: OrderSide.Sell, type: FuturesOrderType.Market, quantity: positionQuantity * 25);
                }
                if (buyVolume < sellVolume && locked == false)
                {
                    if (status == false)
                    {
                        // Sell
                        Console.WriteLine("Sell");
                        positionSide = "SELL";
                        positionPrice = latestPrice;
                        positionQuantity = 200 / latestPrice;
                        status = true;
                        await client.UsdFuturesApi.Trading.PlaceOrderAsync(symbol: "DOGEUSDT", side: OrderSide.Sell, type: FuturesOrderType.Market, quantity: positionQuantity * 25);
                    }
                    else if (status == true)
                    {
                        // Buy Close
                        if (positionSide == "BUY")
                        {
                            score += (positionQuantity * latestPrice) - (positionQuantity * positionPrice);
                            status = false;
                            await client.UsdFuturesApi.Trading.PlaceOrderAsync(symbol: "DOGEUSDT", side: OrderSide.Buy, type: FuturesOrderType.Market, quantity: positionQuantity * 25);
                        }
                    }
                }
                if (oldBuyVolume > buyVolume && positionSide == "BUY")
                {
                    score += (positionQuantity * latestPrice) - (positionQuantity * positionPrice);
                    status = false;
                    await client.UsdFuturesApi.Trading.PlaceOrderAsync(symbol: "DOGEUSDT", side: OrderSide.Buy, type: FuturesOrderType.Market, quantity: positionQuantity * 25);
                }

                oldBuyVolume = buyVolume;
                oldSellVolume = sellVolume;

            }

        }
    }
}
