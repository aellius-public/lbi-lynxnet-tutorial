using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entities.Tutorial;

namespace Aellius.Sandhi.LynX.BusinessProcess
{
    /// <summary>
    /// Summary description for MyProcessClass
    /// This class must implement Aellius.Sandhi.LynX.IIntegrationProcess interface
    /// This class must be decorated with the BusinessProcessAttribute
    /// The IntegrationID property of the attribute must match the Integration ID of the input document.
    /// </summary>
    [BusinessProcess(IntegrationID = "ERP.EOne.Tutorial", Description = "LynX.NET Tutorial")]
    public class TutorialProcess : Aellius.Sandhi.LynX.IIntegrationProcess
    {
        public TutorialProcess()
        {
        
        }

		public void ProcessDocument()
		{
			BusinessDocument bd = (BusinessDocument)DocumentContext.Deserialize(typeof(BusinessDocument));
			try
			{
				GetFormattedAddress(bd);

				DocumentContext.DocumentStatus = true;
			}
			catch (Exception excpt)
			{
				DocumentContext.AddError(excpt, true);
			}
			finally
			{
				// finalize the output
				DocumentContext.FinalizeOutput(bd.document.output, true, true);
			}
		}

		private void GetFormattedAddress(BusinessDocument businessDocument)
		{
			// get name alpha
			string alpha = businessDocument.document.input.NameAlpha;

			// replace * with % for query
			alpha = alpha.Replace("*", "%");
			// create an object of type F0101
			F0101 f0101 = new F0101();

			// build the select statement's where clause
			if (alpha.Contains("%"))
			{
				f0101.AddCriteria(Parenthesis.None, Conjunction.And,
					F0101.Column.NameAlpha, SingleValueOperator.Like, alpha);
			}
			else
			{
				f0101.AddCriteria(Parenthesis.None, Conjunction.And,
					F0101.Column.NameAlpha, SingleValueOperator.EqualTo, alpha);
			}

			// select fields to be retrieved. at least one field must be selected
			f0101.TcAddressNumber.SelectColumn = true;
			f0101.TcNameAlpha.SelectColumn = true;

			// select 
			f0101.Select();

			// fetch
			while (f0101.Fetch())
			{
				// create an object of type FormattedAddress
				FormattedAddress fa = new FormattedAddress();

				// set input values
				fa.DpmnAddressNumber.InValue = f0101.TcAddressNumber.SelectedValue;

				// execute
				BusinessFunctionResult result = fa.Execute();

				// retrieve formatted address and append to output
				if (result == BusinessFunctionResult.Success)
				{
					AddressRecord address = new AddressRecord();
					address.Name = fa.DpszNameMailing.OutValue;
					address.Line1 = fa.DpszAddressLine1.OutValue;
					address.Line2 = fa.DpszAddressLine2.OutValue;
					address.Line3 = fa.DpszAddressLine3.OutValue;
					address.Line4 = fa.DpszAddressLine4.OutValue;
					address.City = fa.DpszCity.OutValue;
					address.State = fa.DpszState.OutValue;
					address.Zip = fa.DpszZipCodePostal.OutValue;

					businessDocument.document.output.Add(address);
				}
			}

			// close the table
			f0101.Close();
		}
	}
}
