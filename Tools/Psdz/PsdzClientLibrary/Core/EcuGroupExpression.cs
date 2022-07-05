﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BMW.Rheingold.CoreFramework.Contracts.Vehicle;

namespace PsdzClient.Core
{
	public class EcuGroupExpression : SingleAssignmentExpression
	{
		public EcuGroupExpression()
		{
		}

		public EcuGroupExpression(long ecuGroupId)
		{
			this.value = ecuGroupId;
		}

		public override bool Evaluate(Vehicle vec, IFFMDynamicResolver ffmResolver, ValidationRuleInternalResults internalResult)
		{
			if (vec == null)
			{
				//Log.Warning("EcuGroupExpression.Evaluate()", "vec was null", Array.Empty<object>());
				return false;
			}
			PdszDatabase.EcuGroup ecuGroupById = ClientContext.GetDatabase(vec)?.GetEcuGroupById(this.value.ToString(CultureInfo.InvariantCulture));
			if (ecuGroupById == null || string.IsNullOrEmpty(ecuGroupById.Name))
			{
				return false;
			}
			if (vec.VCI != null && (vec.VehicleIdentLevel == IdentificationLevel.BasicFeatures || vec.VehicleIdentLevel == IdentificationLevel.VINBasedFeatures || vec.VehicleIdentLevel == IdentificationLevel.VINOnly))
			{
				return true;
			}
			if (vec.VehicleIdentLevel == IdentificationLevel.VINBasedOnlineUpdated && vec.ECU != null && vec.ECU.Count == 0)
			{
				return true;
			}
			bool flag;
			if (!(flag = (vec.getECUbyECU_GRUPPE(ecuGroupById.Name) != null)) && "d_0044".Equals(ecuGroupById.Name, StringComparison.OrdinalIgnoreCase) && vec.BNType == BNType.BN2000_PGO)
			{
				//Log.Info("EcuGroupExpression.Evaluate()", "check for D_0044 (EWS3) => EWS3P", Array.Empty<object>());
				flag = true;
			}
			return flag;
		}

		public override EEvaluationResult EvaluateVariantRule(ClientDefinition client, CharacteristicSet baseConfiguration, EcuConfiguration ecus)
		{
			if (ecus.EcuGroups.ToList<long>().BinarySearch(this.value) >= 0)
			{
				return EEvaluationResult.VALID;
			}
			if (ecus.UnknownEcuGroups.ToList<long>().BinarySearch(this.value) >= 0)
			{
				return EEvaluationResult.MISSING_VARIANT;
			}
			return EEvaluationResult.INVALID;
		}

		public override IList<long> GetUnknownVariantIds(EcuConfiguration ecus)
		{
			List<long> list = new List<long>();
			if (ecus.UnknownEcuGroups.ToList<long>().BinarySearch(this.value) >= 0)
			{
				list.Add(this.value);
			}
			return list;
		}

		public override void Serialize(MemoryStream ms)
		{
			ms.WriteByte(10);
			base.Serialize(ms);
		}

        public override string ToFormula(FormulaConfig formulaConfig)
        {
            PdszDatabase.EcuGroup ecuGroupById = ClientContext.GetDatabase(this.vecInfo)?.GetEcuGroupById(this.value.ToString(CultureInfo.InvariantCulture));
            
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(FormulaSeparator(formulaConfig));
            stringBuilder.Append(formulaConfig.CheckLongFunc);
            stringBuilder.Append("(\"EcuGroup\", ");
            if (ecuGroupById != null)
            {
                stringBuilder.Append(ecuGroupById.DiagAddr.ToString(CultureInfo.InvariantCulture));
            }
            else
            {
                stringBuilder.Append("-1");
            }
            stringBuilder.Append(")");
            stringBuilder.Append(FormulaSeparator(formulaConfig));

            return stringBuilder.ToString();
        }

		public override string ToString()
		{
			return "EcuGroup=" + this.value.ToString(CultureInfo.InvariantCulture);
		}
	}
}
