﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using PsdzClient.Core;

namespace PsdzClient
{
    public class ClientContext : IDisposable
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(ClientContext));
        private bool _disposed;

        public PdszDatabase Database { get; set; }
        public CharacteristicExpression.EnumBrand SelectedBrand { get; set; }
        public string OutletCountry { get; set; }
        public string Language { get; set; }

        public ClientContext()
        {
            Database = null;
            SelectedBrand = CharacteristicExpression.EnumBrand.BMWBMWiMINI;
            OutletCountry = string.Empty;
            Language = "En";
        }

        public static ClientContext GetClientContext(Vehicle vehicle)
        {
            if (vehicle != null)
            {
                if (vehicle.ClientContext == null)
                {
                    log.ErrorFormat("GetClientContext ClientContext is null");
                    return null;
                }

                if (vehicle.ClientContext._disposed)
                {
                    log.ErrorFormat("GetClientContext ClientContext is disposed");
                    return null;
                }

                return vehicle.ClientContext;
            }

            log.ErrorFormat("GetClientContext Vehicle is null");
            return null;
        }

        public static PdszDatabase GetDatabase(Vehicle vehicle)
        {
            ClientContext clientContext = GetClientContext(vehicle);
            if (clientContext == null)
            {
                log.ErrorFormat("GetClientContext ClientContext is null");
                return null;
            }

            if (clientContext.Database == null)
            {
                log.ErrorFormat("GetDatabase Database is null");
                return null;
            }

            return clientContext.Database;
        }

        public static CharacteristicExpression.EnumBrand GetBrand(Vehicle vehicle)
        {
            ClientContext clientContext = GetClientContext(vehicle);
            if (clientContext == null)
            {
                log.ErrorFormat("GetBrand ClientContext is null");
                return CharacteristicExpression.EnumBrand.BMWBMWiMINI;
            }

            return clientContext.SelectedBrand;
        }

        public static string GetCountry(Vehicle vehicle)
        {
            ClientContext clientContext = GetClientContext(vehicle);
            if (clientContext == null)
            {
                log.ErrorFormat("GetCountry ClientContext is null");
                return string.Empty;
            }

            if (clientContext.OutletCountry == null)
            {
                log.ErrorFormat("GetCountry OutletCountry is null");
                return string.Empty;
            }

            return clientContext.OutletCountry;
        }

        public static string GetLanguage(Vehicle vehicle)
        {
            ClientContext clientContext = GetClientContext(vehicle);
            if (clientContext == null)
            {
                log.ErrorFormat("GetLanguage ClientContext is null");
                return string.Empty;
            }

            if (clientContext.Language == null)
            {
                log.ErrorFormat("GetLanguage Language is null");
                return string.Empty;
            }

            return clientContext.Language;
        }

        public void Dispose()
        {
            Dispose(true);
            // This object will be cleaned up by the Dispose method.
            // Therefore, you should call GC.SupressFinalize to
            // take this object off the finalization queue
            // and prevent finalization code for this object
            // from executing a second time.
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!_disposed)
            {
                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (disposing)
                {
                }

                // Note disposing has been done.
                _disposed = true;
            }
        }
    }
}
