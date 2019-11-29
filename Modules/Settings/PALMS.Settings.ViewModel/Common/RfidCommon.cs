﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Impinj.OctaneSdk;

namespace PALMS.Settings.ViewModel.Common
{
    public class RfidCommon
    {
        public ImpinjReader Reader = new ImpinjReader();
        public Impinj.OctaneSdk.Settings settings;

        private ConcurrentDictionary<int, ConcurrentDictionary<string, Tuple<DateTime?, DateTime?>>> _data =
            new ConcurrentDictionary<int, ConcurrentDictionary<string, Tuple<DateTime?, DateTime?>>>();

        public bool Connection()
        {
            try
            {
                if (Reader.IsConnected)
                {
                    Reader.Disconnect();
                }

                Reader.Connect("192.168.250.55");
                Reader.Stop();

            }
            catch (OctaneSdkException ee)
            {
                Console.WriteLine("Octane SDK exception: Reader #1" + ee.Message, "error");
            }
            catch (Exception ee)
            {
                // Handle other .NET errors.
                Console.WriteLine("Exception : Reader #1" + ee.Message, "error");
                Console.WriteLine(ee.StackTrace);
            }

            SetSettings();
            return Reader.IsConnected;
        }

        private void SetSettings()
        {
            settings = Reader.QueryDefaultSettings();

            settings.Report.IncludeAntennaPortNumber = true;
            settings.Report.IncludePhaseAngle = true;
            settings.Report.IncludeChannel = true;
            settings.Report.IncludeDopplerFrequency = true;
            settings.Report.IncludeFastId = true;
            settings.Report.IncludeFirstSeenTime = true;
            settings.Report.IncludeLastSeenTime = true;
            settings.Report.IncludePeakRssi = true;
            settings.Report.IncludeSeenCount = true;
            settings.Report.IncludePcBits = true;
            settings.Report.IncludeSeenCount = true;


            ///ReaderMode.AutoSetDenseReaderDeepScan | Rx = -70 | Tx = 15/20
            ///ReaderMode.MaxThrouput | Rx = -80 | Tx = 15
            settings.ReaderMode = ReaderMode.AutoSetDenseReaderDeepScan;//.AutoSetDenseReader;
            settings.SearchMode = SearchMode.DualTarget;//.DualTarget;
            settings.Session = 1;
            settings.TagPopulationEstimate = Convert.ToUInt16(200);

            settings.Report.Mode = ReportMode.Individual;

            Antenna();

            Reader.ApplySettings(settings);
        }

        public void Antenna()
        {
            settings.Antennas.DisableAll();
            var j = settings.Antennas.AntennaConfigs.Count;

            for (ushort i = 1; i <= j; i++)
            {
                settings.Antennas.GetAntenna(i).IsEnabled = true;
                settings.Antennas.GetAntenna(i).TxPowerInDbm = Convert.ToDouble("15");
                settings.Antennas.GetAntenna(i).RxSensitivityInDbm = Convert.ToDouble("-80");
            }
        }

        public void StartRead()
        {
            Connection();

            Reader.Start();
            Reader.TagsReported += DisplayTag;
        }

        public void StopRead()
        {
            Reader.TagsReported -= DisplayTag;
            Reader.Stop();
            Reader.Disconnect();
        }

        public void ReadDuringTime(int readTime)
        {
            StartRead();

            Thread.Sleep(readTime);

            StopRead();
        }



        public ConcurrentDictionary<int, ConcurrentDictionary<string, Tuple<DateTime?, DateTime?>>> GetFullData()
        {
            if (_data.Count == 0)
            {
                return null;
            }
            return _data;
        }

        public List<Tuple<int,string>> GetSortedData()
        {
            var data = GetFullData();
            var tags = new List<Tuple<int,string>>();

            foreach (var antenna in data)
            {
                tags.AddRange(antenna.Value.Select(tag => new Tuple<int, string>(antenna.Key, tag.Key)));
            }

            return tags;
        }

        public List<string> GetAntennasTags()
        {
            var data = GetFullData();
            var tags = new List<string>();

            foreach (var antenna in data)
            {
                foreach (var tag in antenna.Value)
                {
                    if (tags.Any(x => x == tag.Key))
                    {
                        continue;
                    }
                    tags.Add(tag.Key);
                }
            }

            return tags;
        }

        public List<string> GetAntennaTags(int antNumb)
        {
            var data = GetFullData();
            var tags = new List<string>();

            var antenna = data.FirstOrDefault(x => x.Key == antNumb).Value;

            tags.AddRange(antenna.Select(x=> x.Key));

            return tags;
        }


        private void DisplayTag(ImpinjReader reader, TagReport report)
        {
            foreach (Tag tag in report)
            {
                AddData(tag.AntennaPortNumber, tag.Epc.ToString(), tag.LastSeenTime.LocalDateTime);
            }
        }

        private void AddData(int antenna, string epc, DateTime time)
        {
            // проверка ест ли словарь антенны 
            if (!_data.TryGetValue(antenna, out ConcurrentDictionary<string, Tuple<DateTime?, DateTime?>> val))
            {
                val = new ConcurrentDictionary<string, Tuple<DateTime?, DateTime?>>();

                _data.TryAdd(antenna, val);
            }

            // проверка на наличие чипа в словаре
            if (!val.TryGetValue(epc, out Tuple<DateTime?, DateTime?> times))
            {
                times = new Tuple<DateTime?, DateTime?>(time, null);
                val.TryAdd(epc, times);
            }
            else
            {
                val.TryUpdate(epc, new Tuple<DateTime?, DateTime?>(times.Item1, time), times);
                //данные можно сохранять в БД, но метке можно обнулить
            }

            // если метка повторно падает в считыватель, то нужно предыдущие данные сохранять в БД
        }

    }
}
