using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace RJISFlow
{
    class Program
    {
        private static Dictionary<string, List<FlowValue>> flowDic;
        private static Dictionary<int, List<Ticket>> ticketDic;


        static void Main(string[] args)
        {
            var start = DateTime.Now;
            var firstTextFile = new DirectoryInfo(@"s:/").EnumerateFiles("*.ffl")
                                                                .FirstOrDefault();

            if (firstTextFile != null)
            {
                Console.WriteLine("first file is " + firstTextFile.Name);

                flowDic = new Dictionary<string, List<FlowValue>>();
                ticketDic = new Dictionary<int, List<Ticket>>();

                var ticketSet = new HashSet<string> { "7DF", "7DS", "ADT", "CBA", "CDR", "CDS", "CNR", "CNS", "ECD", "EFS", "EVS", "FCR", "FDA", "FDB", "FDC", "FDD", "FDR", "FDS", "FOR", "FOS", "FSR", "FSS", "G1R", "G1S", "G2R", "G2S", "GTR", "GTS", "GUR", "GUS", "LCF", "ODT", "SCO", "SDR", "SDS", "SFR", "SOR", "SOS", "SSR", "SSS", "STO", "SVR", "SVS", "WKR"};

                using (var reader = new StreamReader(firstTextFile.FullName))
                {
                    var linenumber = 0;
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (line.Substring(0, 2) == "RF")
                        {
                            var flow = new Flow(line);
                            var flowValue = new FlowValue(line);
                            if (flowValue.EndDate.Date >= DateTime.Now.Date)
                            {
                                DictUtils.AddEntry(flowDic, flow.FlowKey, flowValue);
                                if (line[19] == 'R')
                                {
                                    DictUtils.AddEntry(flowDic, flow.GetReversedFlow().FlowKey, flowValue);
                                }
                            }
                        }
                        else if (line.Substring(0, 2) == "RT")
                        {
                            var key = Convert.ToInt32(line.Substring(2, 7));
                            var ticketValue = new Ticket(line);
                            if (ticketSet.Contains(ticketValue.TicketCode))
                            {
                                DictUtils.AddEntry(ticketDic, key, ticketValue);
                            }
                        }
                        linenumber++;
                    }

                    var flowidSet = new HashSet<int>(from x in flowDic.Values from flowv in x select flowv.FlowId);
                    var fareFlowidSet = new HashSet<int>(ticketDic.Keys);
                    var unusedTicketKeys = ticketDic.Keys.Where(x => !flowidSet.Contains(x));

                    var keysToRemove = new List<string>();
                    foreach (var flow in flowDic)
                    {
                        flow.Value.RemoveAll(x => !fareFlowidSet.Contains(x.FlowId));
                        if (flow.Value.Count == 0)
                        {
                            keysToRemove.Add(flow.Key);
                        }
                    }
                    foreach (var key in keysToRemove)
                    {
                        flowDic.Remove(key);
                    }
                    GC.Collect();
                    GC.WaitForPendingFinalizers();

                    Console.WriteLine("dictionary size is " + flowDic.Count.ToString());
                    var result = flowDic.TryGetValue("Q202Q839", out var flowresults);
                    if (result)
                    {
                        foreach (var flow in flowresults)
                        {
                            Console.WriteLine("start date: {0} end date: {1} flow id: {2}", flow.StartDate, flow.EndDate, flow.FlowId);
                            if (ticketDic.TryGetValue(flow.FlowId, out var tickets))
                            {
                                foreach (var ticket in tickets)
                                {
                                    Console.WriteLine($"ticket: {ticket.TicketCode} price: {ticket.Price / 100:C}");
                                }
                            }
                        }
                    }
                    Console.WriteLine("Finished");
                    var end = DateTime.Now;
                    var duration = (end - start).TotalMilliseconds;
                    Console.WriteLine($"{duration} milliseconds");
                }
            }
        }
    }
}
