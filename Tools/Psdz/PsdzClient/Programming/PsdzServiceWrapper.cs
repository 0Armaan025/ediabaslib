﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PsdzClient.Psdz;
using PsdzClient.Vehicle;

namespace PsdzClient.Programming
{
	public class PsdzServiceWrapper : IPsdz, IPsdzService, IPsdzInfo, IDisposable
	{
		public PsdzServiceWrapper(PsdzConfig psdzConfig)
		{
			if (psdzConfig == null)
			{
				throw new ArgumentNullException("psdzConfig");
			}
			this.psdzHostPath = psdzConfig.HostPath;
			this.psdzServiceArgs = psdzConfig.PsdzServiceArgs;
			this.psdzServiceHostLogDir = psdzConfig.PsdzServiceHostLogDir;
			this.psdzServiceClient = new PsdzServiceClient(psdzConfig.ClientLogPath);
			this.ObjectBuilder = new PsdzObjectBuilder(this.psdzServiceClient.ObjectBuilderService);
		}

		public IConfigurationService ConfigurationService
		{
			get
			{
				return this.psdzServiceClient.ConfigurationService;
			}
		}

		public IConnectionFactoryService ConnectionFactoryService
		{
			get
			{
				return this.psdzServiceClient.ConnectionFactoryService;
			}
		}

		public IConnectionManagerService ConnectionManagerService
		{
			get
			{
				return this.psdzServiceClient.ConnectionManagerService;
			}
		}

		public IEcuService EcuService
		{
			get
			{
				return this.psdzServiceClient.EcuService;
			}
		}

		public IEventManagerService EventManagerService
		{
			get
			{
				return this.psdzServiceClient.EventManagerService;
			}
		}

		public string ExpectedPsdzVersion { get; private set; }

		public bool IsPsdzInitialized
		{
			get
			{
				return PsdzServiceStarter.IsServerInstanceRunning();
			}
		}

        public bool IsValidPsdzVersion
        {
            get { return true; }
        }

		public ILogService LogService
		{
			get
			{
				return this.psdzServiceClient.LogService;
			}
		}

		public ILogicService LogicService
		{
			get
			{
				return this.psdzServiceClient.LogicService;
			}
		}

		public IMacrosService MacrosService
		{
			get
			{
				return this.psdzServiceClient.MacrosService;
			}
		}

		public IPsdzObjectBuilder ObjectBuilder { get; private set; }

		public IObjectBuilderService ObjectBuilderService
		{
			get
			{
				return this.psdzServiceClient.ObjectBuilderService;
			}
		}

		public Psdz.IProgrammingService ProgrammingService
		{
			get
			{
				return this.psdzServiceClient.ProgrammingService;
			}
		}

		public string PsdzDataPath
		{
			get
			{
				return this.psdzServiceClient.ConfigurationService.GetRootDirectory();
			}
		}

		public string PsdzVersion { get; private set; }

		public ITalExecutionService TalExecutionService
		{
			get
			{
				return this.psdzServiceClient.TalExecutionService;
			}
		}

		public IVcmService VcmService
		{
			get
			{
				return this.psdzServiceClient.VcmService;
			}
		}

		public ICbbTlsConfiguratorService CbbTlsConfiguratorService
		{
			get
			{
				return this.psdzServiceClient.CbbTlsConfiguratorService;
			}
		}

		public ICertificateManagementService CertificateManagementService
		{
			get
			{
				return this.psdzServiceClient.CertificateManagementService;
			}
		}

		public IIndividualDataRestoreService IndividualDataRestoreService
		{
			get
			{
				return this.psdzServiceClient.IndividualDataRestoreService;
			}
		}

		public ISecureFeatureActivationService SecureFeatureActivationService
		{
			get
			{
				return this.psdzServiceClient.SecureFeatureActivationService;
			}
		}

		public ISecurityManagementService SecurityManagementService
		{
			get
			{
				return this.psdzServiceClient.SecurityManagementService;
			}
		}

		public ISecureCodingService SecureCodingService
		{
			get
			{
				return this.psdzServiceClient.SecureCodingService;
			}
		}

		public IKdsService KdsService
		{
			get
			{
				return this.psdzServiceClient.KdsService;
			}
		}

		public void AddPsdzEventListener(IPsdzEventListener psdzEventListener)
		{
			this.psdzServiceClient.AddPsdzEventListener(psdzEventListener);
		}

		public void AddPsdzProgressListener(IPsdzProgressListener progressListener)
		{
			this.psdzServiceClient.AddPsdzProgressListener(progressListener);
		}

		public void CloseConnectionsToPsdzHost()
		{
			if (this.IsPsdzInitialized)
			{
				this.psdzServiceClient.CloseAllConnections();
			}
		}

		public void Dispose()
		{
			if (this.IsPsdzInitialized)
			{
				this.psdzServiceClient.Dispose();
			}
		}

		public void RemovePsdzEventListener(IPsdzEventListener psdzEventListener)
		{
			this.psdzServiceClient.RemovePsdzEventListener(psdzEventListener);
		}

		public void RemovePsdzProgressListener(IPsdzProgressListener progressListener)
		{
			this.psdzServiceClient.RemovePsdzProgressListener(progressListener);
		}

		public void SetLogLevel(PsdzLoglevel psdzLoglevel, ProdiasLoglevel prodiasLoglevel)
		{
			this.psdzLoglevel = new PsdzLoglevel?(psdzLoglevel);
			this.prodiasLoglevel = new ProdiasLoglevel?(prodiasLoglevel);
			if (this.IsPsdzInitialized)
			{
				this.psdzServiceClient.LogService.SetLogLevel(psdzLoglevel);
				this.psdzServiceClient.ConnectionManagerService.SetProdiasLogLevel(prodiasLoglevel);
			}
		}

		public bool StartHostIfNotRunning(IVehicle vehicle = null)
		{
			try
			{
				if (this.IsPsdzInitialized)
				{
					this.DoSettingsForInitializedPsdz();
				}
				else
				{
					PsdzServiceStarter.PsdzServiceStartResult psdzServiceStartResult = new PsdzServiceStarter(this.psdzHostPath, this.psdzServiceHostLogDir, this.psdzServiceArgs).StartIfNotRunning();
					switch (psdzServiceStartResult)
					{
						case PsdzServiceStarter.PsdzServiceStartResult.PsdzStillRunning:
						case PsdzServiceStarter.PsdzServiceStartResult.PsdzStartOk:
							this.DoSettingsForInitializedPsdz();
							break;
						default:
                            return false;
						case PsdzServiceStarter.PsdzServiceStartResult.PsdzStartFailedMemError:
                            return false;
					}
				}
			}
			catch (Exception)
			{
                return false;
			}

            return true;
        }

		public void DoInitSettings()
		{
			if (this.IsPsdzInitialized && this.PsdzVersion == null && this.ExpectedPsdzVersion == null)
			{
				this.DoSettingsForInitializedPsdz();
			}
		}

		public bool Shutdown()
		{
			try
			{
				if (this.IsPsdzInitialized)
				{
					this.CloseConnectionsToPsdzHost();
					this.ConnectionManagerService.RequestShutdown();
				}
			}
			catch (Exception)
            {
                return false;
            }

            return true;
        }

		private void DoSettingsForInitializedPsdz()
		{
            this.psdzLoglevel = PsdzLoglevel.DEBUG;
			if (this.psdzLoglevel != null && !this.psdzServiceArgs.IsTestRun)
			{
				this.psdzServiceClient.LogService.SetLogLevel(this.psdzLoglevel.Value);
			}
			this.prodiasLoglevel = ProdiasLoglevel.DEBUG;
			if (this.prodiasLoglevel != null)
			{
				this.psdzServiceClient.ConnectionManagerService.SetProdiasLogLevel(this.prodiasLoglevel.Value);
			}
			this.ExpectedPsdzVersion = this.psdzServiceClient.ConfigurationService.GetExpectedPsdzVersion();
			this.PsdzVersion = this.psdzServiceClient.ConfigurationService.GetPsdzVersion();
		}

		private readonly PsdzServiceArgs psdzServiceArgs;

		private readonly PsdzServiceClient psdzServiceClient;

		private ProdiasLoglevel? prodiasLoglevel = new ProdiasLoglevel?(ProdiasLoglevel.ERROR);

		private PsdzLoglevel? psdzLoglevel = new PsdzLoglevel?(PsdzLoglevel.FINE);

		private readonly string psdzHostPath;

		private readonly string psdzServiceHostLogDir;
	}
}
