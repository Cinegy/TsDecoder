/* Copyright 2017 Cinegy GmbH.

  Licensed under the Apache License, Version 2.0 (the "License");
  you may not use this file except in compliance with the License.
  You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

  Unless required by applicable law or agreed to in writing, software
  distributed under the License is distributed on an "AS IS" BASIS,
  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
  See the License for the specific language governing permissions and
  limitations under the License.
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Cinegy.TsDecoder.Tables;

namespace Cinegy.TsDecoder.TransportStream
{
    public class TsDecoder
    {

        public ProgramAssociationTable ProgramAssociationTable => _patFactory.ProgramAssociationTable;
        public ServiceDescriptionTable ServiceDescriptionTable => _sdtFactory.ServiceDescriptionTable;
        public ServiceDescriptionTable OtherServiceDescriptionTable => _otherSdtFactory.ServiceDescriptionTable;
        public NetworkInformationTable NetworkInformationTable => _nitFactory.NetworkInformationTable;
        public EventInformationTable EventInformationTable => _eitFactory.EventInformationTable;
        public SpliceInfoTable SpliceInfoTable => _sitFactory.SpliceInfoTable;

        public List<ProgramMapTable> ProgramMapTables { get; private set; }
        
        private ProgramAssociationTableFactory _patFactory;
        private ServiceDescriptionTableFactory _sdtFactory;
        private ServiceDescriptionTableFactory _otherSdtFactory;
        private List<ProgramMapTableFactory> _pmtFactories;

        private EventInformationTableFactory _eitFactory;
        private NetworkInformationTableFactory _nitFactory;
        private SpliceInfoTableFactory _sitFactory;
        
        private TsPacketFactory _packetFactory;

        public delegate void TableChangeEventHandler(object sender, TableChangedEventArgs args);

        public TsDecoder()
        {
#if !NET461
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
#endif
            SetupFactories();
        }

        public void AddData(byte[] data)
        {
            if(_packetFactory == null) _packetFactory = new TsPacketFactory();

            var tsPackets = _packetFactory.GetTsPacketsFromData(data);
            
            if (tsPackets == null)
            {
                throw new InvalidDataException("Provided data buffer did not contain any TS packets");
            }

            foreach (var packet in tsPackets)
            {
                AddPacket(packet);
            }

        }

        public void AddPackets(IEnumerable<TsPacket> newPackets)
        {
            if (newPackets == null) return;

            foreach (var newPacket in newPackets)
            {
                AddPacket(newPacket);
            }
        }
        
        public void AddPacket(TsPacket newPacket)
        {
            try
            {
                if (newPacket.TransportErrorIndicator)
                {
                    return;
                }

                switch (newPacket.Pid)
                {
                    case (short)PidType.PatPid:
                        _patFactory.AddPacket(newPacket);
                        break;
                    case (short)PidType.SdtBatPid:
                        _sdtFactory.AddPacket(newPacket);
                        _otherSdtFactory.AddPacket(newPacket);
                        break;
                    case (short)PidType.EitPid:
                        _eitFactory.AddPacket(newPacket);
                        break;
                    case 2048:
                        _sitFactory.AddPacket(newPacket);
                        break;
                    default:
                        CheckPmt(newPacket);
                        break;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception generated within AddPacket method: " + ex.Message);
            }
        }

        public ServiceDescriptor GetServiceDescriptorForProgramNumber(int? programNumber)
        {
            if (programNumber == null)
                programNumber = _patFactory?.ProgramAssociationTable?.ProgramNumbers?.Where(i => i > 0).FirstOrDefault();

            if (programNumber == 0) return null;

            var serviceDescItem = _sdtFactory?.ServiceDescriptionItems?.SingleOrDefault(
                                  i => i.ServiceId == programNumber);

            var serviceDesc =
                serviceDescItem?.Descriptors?.SingleOrDefault(sd => (sd as ServiceDescriptor) != null) as ServiceDescriptor;

            return serviceDesc;
        }

        public T GetDescriptorForProgramNumberByTag<T>( int? programNumber, int streamType, int descriptorTag, bool firstOfMany = false)  where T : class
        {
            if (programNumber == null) return null;
            
            var selectedPmt = ProgramMapTables?.FirstOrDefault(t => t.ProgramNumber == programNumber);

            if (selectedPmt == null) return null;

            var selectedDesc = default(T);

            foreach (var esStream in selectedPmt.EsStreams)
            {
                if (esStream.StreamType != streamType) continue;

                if (firstOfMany)
                {
                    selectedDesc = esStream.Descriptors.FirstOrDefault(d => d.DescriptorTag == descriptorTag) as T;
                }
                else
                {
                    selectedDesc = esStream.Descriptors.SingleOrDefault(d => d.DescriptorTag == descriptorTag) as T;
                }

                if (selectedDesc != null) break;
            }
        
            return selectedDesc;
            
        }

        public EsInfo GetEsStreamForProgramNumberByTag(int? programNumber, int streamType, int descriptorTag) 
        {
            if (programNumber == null) return null;

            var selectedPmt = ProgramMapTables?.FirstOrDefault(t => t.ProgramNumber == programNumber);

            if (selectedPmt == null) return null;

            foreach (var esStream in selectedPmt.EsStreams)
            {
                if (esStream.StreamType != streamType) continue;

                var desc = esStream.Descriptors.FirstOrDefault(d => d.DescriptorTag == descriptorTag);

                if (desc != null) return esStream;
               
            }

            return null;
        }

        private void CheckPmt(TsPacket tsPacket)
        {
            if (ProgramAssociationTable == null) return;

            if (tsPacket.Pid == (short)PidType.NitPid)
            {
                _nitFactory.AddPacket(tsPacket);
                return;
            }

           // CheckPcr(tsPacket);

            var contains = false;

            foreach (var pid in ProgramAssociationTable.Pids)
            {
                if (pid != tsPacket.Pid) continue;
                contains = true;
                break;
            }

            if (!contains) return;

            ProgramMapTableFactory selectedPmt = null;
            foreach (var t in _pmtFactories)
            {
                if (t.TablePid != tsPacket.Pid) continue;
                selectedPmt = t;
                break;
            }

            if (selectedPmt == null)
            {
                selectedPmt = new ProgramMapTableFactory();
                selectedPmt.TableChangeDetected += _pmtFactory_TableChangeDetected;
                _pmtFactories?.Add(selectedPmt);
            }

            selectedPmt.AddPacket(tsPacket);
        }

     
        private void SetupFactories()
        {
            _patFactory = new ProgramAssociationTableFactory();
            _patFactory.TableChangeDetected += _patFactory_TableChangeDetected;
            _pmtFactories = new List<ProgramMapTableFactory>(16);
            ProgramMapTables = new List<ProgramMapTable>(16);

            _sdtFactory = new ServiceDescriptionTableFactory();
            _sdtFactory.TableChangeDetected += _sdtFactory_TableChangeDetected;

            _otherSdtFactory = new ServiceDescriptionTableFactory { CurrentMux = false };
            _otherSdtFactory.TableChangeDetected += _otherSdtFactory_TableChangeDetected;

            _eitFactory = new EventInformationTableFactory();
            _eitFactory.TableChangeDetected += _eitFactory_TableChangeDetected;

            _nitFactory = new NetworkInformationTableFactory();
            _nitFactory.TableChangeDetected += _nitFactory_TableChangeDetected;

            _sitFactory = new SpliceInfoTableFactory();
        }

        private void _otherSdtFactory_TableChangeDetected(object sender, TransportStreamEventArgs e)
        {
            try
            {
                var fact = sender as ServiceDescriptionTableFactory;
                var message =
                    $"SDT (other mux) Refreshed (Version {OtherServiceDescriptionTable?.VersionNumber}, Section {OtherServiceDescriptionTable?.SectionNumber}, Channels: {OtherServiceDescriptionTable?.Items.Count})";

                OnTableChangeDetected(fact, new TableChangedEventArgs() { Message = message, TablePid = e.TsPid, TableType = TableType.SdtOther });
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Problem reading service name: " + ex.Message);
            }
        }

        public ProgramMapTable GetSelectedPmt(int programNumber = 0)
        {
            ProgramMapTable pmt;

            if (programNumber == 0)
            {
                if (ProgramMapTables?.Count == 0) return null;
                if (ProgramAssociationTable == null) return null;
                //without a passed program number, use the default program
                if (ProgramMapTables?.Count <
                    (ProgramAssociationTable?.Pids?.Length - 1)) return null;

                pmt = ProgramMapTables?.OrderBy(t => t.ProgramNumber).FirstOrDefault();
            }
            else
            {
                pmt = ProgramMapTables?.SingleOrDefault(t => t.ProgramNumber == programNumber);
            }

            return pmt;
        }

        private void _sdtFactory_TableChangeDetected(object sender, TransportStreamEventArgs e)
        {
            try
            {
                var fact = sender as ServiceDescriptionTableFactory;
                var name = GetServiceDescriptorForProgramNumber(ProgramMapTables.FirstOrDefault()?.ProgramNumber);
                var message =
                    $"SDT (this mux) Refreshed: {name?.ServiceName} - {name?.ServiceProviderName} (Version {ServiceDescriptionTable?.VersionNumber}, Section {ServiceDescriptionTable?.SectionNumber})";

                OnTableChangeDetected(fact, new TableChangedEventArgs() { Message = message, TablePid = e.TsPid, TableType = TableType.Sdt});
            }
            catch(Exception ex)
            {
                Debug.WriteLine("Problem reading service name: " + ex.Message);   
            }
        }

        private void _pmtFactory_TableChangeDetected(object sender, TransportStreamEventArgs e)
        {
            string message;
            lock (this)
            {
                var fact = sender as ProgramMapTableFactory;

                if (fact == null) return;

                var selectedPmt = ProgramMapTables?.FirstOrDefault(t => t.Pid == e.TsPid);

                if (selectedPmt != null)
                {
                    ProgramMapTables?.Remove(selectedPmt);
                    message = $"PMT {e.TsPid} refreshed";
                }
                else
                {
                    message = $"PMT {e.TsPid} added";
                }

                ProgramMapTables?.Add(fact.ProgramMapTable);
                OnTableChangeDetected(fact, new TableChangedEventArgs() { Message = message, TablePid = e.TsPid, TableType = TableType.Pmt });
            }

        }

        private void _patFactory_TableChangeDetected(object sender, TransportStreamEventArgs e)
        {
            _pmtFactories = new List<ProgramMapTableFactory>(16);
            ProgramMapTables = new List<ProgramMapTable>(16);

            _sdtFactory = new ServiceDescriptionTableFactory();
            _sdtFactory.TableChangeDetected += _sdtFactory_TableChangeDetected;

            OnTableChangeDetected(null, new TableChangedEventArgs() {Message = "PAT refreshed - resetting all factories" , TablePid = e.TsPid, TableType = TableType.Pat});
        }

        private void _eitFactory_TableChangeDetected(object sender, TransportStreamEventArgs e)
        {
            string message;
            lock (this)
            {
                var fact = sender as EventInformationTableFactory;

                if (fact == null) return;
                message = $"EIT {e.TsPid} Refreshed:";

                OnTableChangeDetected(fact, new TableChangedEventArgs() { Message = message, TablePid = e.TsPid, TableType = TableType.Eit });
            }

        }

        private void _nitFactory_TableChangeDetected(object sender, TransportStreamEventArgs e)
        {
            string message;
            lock (this)
            {
                var fact = sender as NetworkInformationTableFactory;

                if (fact == null) return;
                message = $"NIT {e.TsPid} Refreshed: (Version {NetworkInformationTable?.VersionNumber}, Section {NetworkInformationTable?.SectionNumber})";

                OnTableChangeDetected(fact, new TableChangedEventArgs() { Message = message, TablePid = e.TsPid, TableType = TableType.Nit });
            }

        }

        //A decoded table change has been processed
        public event TableChangeEventHandler TableChangeDetected;

        private void OnTableChangeDetected(TableFactory sourceTableFactory, TableChangedEventArgs args)
        {   
            var handler = TableChangeDetected;
            handler?.Invoke(sourceTableFactory, args);
        }
    }


}

