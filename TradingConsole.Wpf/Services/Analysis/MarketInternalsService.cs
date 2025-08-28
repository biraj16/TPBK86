// TradingConsole.Wpf/Services/Analysis/MarketInternalsService.cs
using System;
using System.Collections.Generic;
using System.Linq;
using TradingConsole.Core.Models;

namespace TradingConsole.Wpf.Services.Analysis
{
    public class MarketInternalsService
    {
        private readonly Dictionary<string, double> _niftyHeavyweights = new Dictionary<string, double>
        {
            { "RELIANCE", 11.5 },
            { "HDFCBANK", 8.5 },
            { "ICICIBANK", 7.9 },
            { "INFY", 5.8 },
            { "TCS", 4.5 },
            { "BHARTIARTL", 4.0 },
            { "LARTURBO", 3.8 }
        };

        public int CalculateParticipationScore(DashboardInstrument nifty, IEnumerable<DashboardInstrument> allInstruments)
        {
            if (nifty.Open == 0) return 50; // Return neutral if no data yet

            var heavyweightsData = allInstruments
                .Where(inst => inst.UnderlyingSymbol != null && _niftyHeavyweights.ContainsKey(inst.UnderlyingSymbol.ToUpper()))
                .ToList();

            if (!heavyweightsData.Any()) return 0; // No heavyweights being tracked

            double totalWeight = 0;
            double weightedChange = 0;

            foreach (var stock in heavyweightsData)
            {
                if (stock.Open > 0)
                {
                    var stockWeight = _niftyHeavyweights[stock.UnderlyingSymbol.ToUpper()];
                    var stockPctChange = (double)(stock.LTP - stock.Open) / (double)stock.Open;
                    weightedChange += stockPctChange * stockWeight;
                    totalWeight += stockWeight;
                }
            }

            if (totalWeight == 0) return 50;

            var weightedAvgHeavyweightChange = weightedChange / totalWeight;
            var niftyPctChange = (double)(nifty.LTP - nifty.Open) / (double)nifty.Open;

            if (Math.Sign(niftyPctChange) != Math.Sign(weightedAvgHeavyweightChange) && Math.Abs(niftyPctChange) > 0.001)
            {
                return 10; // Strong divergence
            }

            double difference = Math.Abs(niftyPctChange - weightedAvgHeavyweightChange);
            int score = 100 - (int)(Math.Min(difference, 0.01) * 10000);

            return Math.Max(0, Math.Min(100, score));
        }
    }
}