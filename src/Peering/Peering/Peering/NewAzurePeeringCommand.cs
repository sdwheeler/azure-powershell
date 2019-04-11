﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright company="Microsoft" file="NewAzurePeeringCommand.cs">
//   Licensed under the Apache License, Version 2.0 (the "License");
//   //   you may not use this file except in compliance with the License.
//   //   You may obtain a copy of the License at
//   //   http://www.apache.org/licenses/LICENSE-2.0
//   //   Unless required by applicable law or agreed to in writing, software
//   //   distributed under the License is distributed on an "AS IS" BASIS,
//   //   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   //   See the License for the specific language governing permissions and
//   //   limitations under the License.
// </copyright>
// <summary>
//   
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Microsoft.Azure.PowerShell.Cmdlets.Peering.Peering
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Management.Automation;
    using System.Net.Http;
    using System.Reflection.Emit;

    using Microsoft.Azure.Commands.ResourceManager.Common.ArgumentCompleters;
    using Microsoft.Azure.Management.Peering;
    using Microsoft.Azure.Management.Peering.Models;
    using Microsoft.Azure.PowerShell.Cmdlets.Peering.Common;
    using Microsoft.Azure.PowerShell.Cmdlets.Peering.Models;

    /// <summary>
    /// New Azure InputObject Command-let
    /// </summary>
    [Cmdlet(
        VerbsCommon.New,
        "AzPeering",
        DefaultParameterSetName = Constants.Exchange,
        SupportsShouldProcess = true)]
    [OutputType(typeof(PSPeering))]
    public class NewAzurePeeringCommand : PeeringBaseCmdlet
    {
        /// <summary>
        /// Gets or sets the legacy InputObject.
        /// </summary>
        [Parameter(
            Mandatory = true,
            ValueFromPipeline = true,
            ParameterSetName = Constants.ParameterSetNameConvertLegacyPeering,
            DontShow = true)]
        public PSPeering LegacyPeering { get; set; }

        /// <summary>
        /// Gets or sets The Resource Group Name
        /// </summary>
        [Parameter(
            Position = 0,
            Mandatory = true,
            HelpMessage = Constants.ResourceGroupNameHelp,
            ParameterSetName = Constants.ParameterSetNameConvertLegacyPeering)]
        [Parameter(
            Position = 0,
            Mandatory = true,
            HelpMessage = Constants.ResourceGroupNameHelp,
            ParameterSetName = Constants.Exchange)]
        [Parameter(
            Position = 0,
            Mandatory = true,
            HelpMessage = Constants.ResourceGroupNameHelp,
            ParameterSetName = Constants.Direct)]
        [ResourceGroupCompleter]
        [ValidateNotNullOrEmpty]
        public virtual string ResourceGroupName { get; set; }

        /// <summary>
        /// Gets or sets The InputObject NameMD5AuthenticationKeyHelp
        /// </summary>
        [Parameter(
            Position = 1,
            Mandatory = true,
            HelpMessage = Constants.PeeringNameHelp,
            ParameterSetName = Constants.ParameterSetNameConvertLegacyPeering)]
        [Parameter(
            Position = 1,
            Mandatory = true,
            HelpMessage = Constants.PeeringNameHelp,
            ParameterSetName = Constants.Exchange)]
        [Parameter(
            Position = 1,
            Mandatory = true,
            HelpMessage = Constants.PeeringNameHelp,
            ParameterSetName = Constants.Direct)]
        [ValidateNotNullOrEmpty]
        public virtual string Name { get; set; }

        /// <summary>
        /// Gets or sets The InputObject Location.
        /// </summary>
        [Parameter(
            Mandatory = false,
            HelpMessage = Constants.PeeringLocationHelp,
            ParameterSetName = Constants.ParameterSetNameConvertLegacyPeering)]
        [Parameter(
            Position = 2,
            Mandatory = true,
            HelpMessage = Constants.PeeringLocationHelp,
            ParameterSetName = Constants.Exchange)]
        [Parameter(
            Position = 2,
            Mandatory = true,
            HelpMessage = Constants.PeeringLocationHelp,
            ParameterSetName = Constants.Direct)]
        [ValidateNotNullOrEmpty]
        public virtual string PeeringLocation { get; set; }

        /// <summary>
        /// Gets or sets The PeerAsn.
        /// </summary>
        [Parameter(
            Position = 3,
            Mandatory = true,
            HelpMessage = Constants.PeeringAsnHelp,
            ParameterSetName = Constants.ParameterSetNameConvertLegacyPeering)]
        [Parameter(
            Position = 3,
            Mandatory = true,
            HelpMessage = Constants.PeeringAsnHelp,
            ParameterSetName = Constants.Exchange)]
        [Parameter(
            Position = 3,
            Mandatory = true,
            HelpMessage = Constants.PeeringAsnHelp,
            ParameterSetName = Constants.Direct)]
        [ValidateNotNullOrEmpty]
        public virtual string PeerAsnResourceId { get; set; }

        /// <summary>
        /// Gets or sets the exchange session.
        /// </summary>
        [Parameter(
            Mandatory = true,
            ValueFromPipeline = true,
            HelpMessage = Constants.PeeringExchangeConnectionHelp,
            ParameterSetName = Constants.Exchange)]
        [ValidateNotNull]
        public virtual PSExchangeConnection[] ExchangeConnection { get; set; }

        /// <summary>
        /// Gets or sets the direct session.
        /// </summary>
        [Parameter(
            Mandatory = true,
            ValueFromPipeline = true,
            HelpMessage = Constants.PeeringDirectConnectionHelp,
            ParameterSetName = Constants.Direct)]
        [ValidateNotNull]
        public virtual PSDirectConnection[] DirectConnection { get; set; }

        [Parameter(
            Mandatory = false,
            HelpMessage = Constants.UseForPeeringServiceHelp,
            ParameterSetName = Constants.Direct)]
        public virtual SwitchParameter UseForPeeringService { get; set; }

        /// <summary>
        /// Gets or sets the tag.
        /// </summary>
        [Parameter(Mandatory = false,
            HelpMessage = Constants.TagsHelp)]
        public virtual Hashtable Tag { get; set; }

        /// <summary>
        ///     The AsJob parameter to run in the background.
        /// </summary>
        [Parameter(Mandatory = false, HelpMessage = Constants.AsJobHelp)]
        public SwitchParameter AsJob { get; set; }

        /// <summary>
        /// The inherited Execute function.
        /// </summary>
        public override void Execute()
        {
            try
            {
                base.Execute();
                if (this.ParameterSetName.Equals(Constants.Exchange, StringComparison.OrdinalIgnoreCase))
                    this.WriteObject(new PSExchangePeeringModelView(this.CreateExchangePeering()));

                if (this.ParameterSetName.Equals(Constants.Direct, StringComparison.OrdinalIgnoreCase))
                    this.WriteObject(new PSDirectPeeringModelView(this.CreateDirectPeering()));

                if (this.ParameterSetName.Equals(Constants.ParameterSetNameConvertLegacyPeering))
                {
                    if (this.LegacyPeering != null)
                    {
                        if (this.LegacyPeering.Exchange != null && string.Equals(this.LegacyPeering.Kind, Constants.Exchange, StringComparison.OrdinalIgnoreCase))
                        {
                            this.WriteObject(new PSExchangePeeringModelView((PSPeering)this.PutNewPeering(this.ConvertClassicToExchangePeering(this.LegacyPeering))));
                        }
                        if (this.LegacyPeering.Direct != null && string.Equals(this.LegacyPeering.Kind, Constants.Direct, StringComparison.OrdinalIgnoreCase))
                        {
                            this.WriteObject(new PSDirectPeeringModelView((PSPeering)this.PutNewPeering(this.ConvertClassicToDirectPeering(this.LegacyPeering))));
                        }
                    }
                }
            }
            catch (InvalidOperationException mapException)
            {
                throw new InvalidOperationException($"Failed to map object {mapException}");
            }
            catch (ErrorResponseException ex)
            {
                throw new ErrorResponseException($"{ex}");
            }
        }

        /// <summary>
        /// The create direct peering.
        /// </summary>
        /// <returns>
        /// The <see cref="PSPeering"/>.
        /// </returns>
        /// <exception cref="PSArgumentNullException">
        /// </exception>
        /// <exception cref="PSArgumentException">
        /// </exception>
        /// <exception cref="Exception">
        /// </exception>
        /// <exception cref="ArmErrorException">
        /// </exception>
        /// <exception cref="HttpRequestException">
        /// </exception>
        private PSPeering CreateDirectPeering()
        {

            var peeringRequest =
                new PSPeering(
                    name: this.Name,
                    location: this.GetAzureRegion(this.PeeringLocation, Constants.Direct),
                    sku: this.UseForPeeringService ? new PSPeeringSku { Name = Constants.PremiumDirectFree } : new PSPeeringSku { Name = Constants.BasicDirectFree },
                    kind: Constants.Direct)
                {
                    PeeringLocation = this.PeeringLocation,
                    Direct = new PSPeeringPropertiesDirect
                    {
                        UseForPeeringService = this.UseForPeeringService,
                        Connections = new List<PSDirectConnection>(),
                        PeerAsn = new PSSubResource(this.PeerAsnResourceId)
                    },
                    Tags = this.ConvertToDictionary(this.Tag),
                };
            if (this.DirectConnection == null)
            {
                throw new PSArgumentNullException($"Direct connection Cannot be null. Create a new PSDirectPeeringConnection by using the New-AzDirectPeeringConnection cmdlet.");
            }

            foreach (var psDirectConnection in this.DirectConnection)
            {
                if (this.ValidConnection(psDirectConnection))
                    peeringRequest.Direct.Connections.Add(psDirectConnection);
            }

            if (this.ParameterSetName.Equals(Constants.Direct))
            {
                if (peeringRequest.Direct?.Connections.Count <= 0)
                    throw new PSArgumentException($"Direct Connection cannot be null.");
                if (peeringRequest.Exchange != null)
                {
                    peeringRequest.Exchange = null;
                }
                try
                {
                    // For Testing Only
                    //return peeringRequest;
                    // Commented for Testing Purposes Remove for Production
                    return (PSPeering)this.PutNewPeering(peeringRequest);
                }
                catch (ErrorResponseException ex)
                {
                    throw new Exception($"Error:{ex.Response.ReasonPhrase} reason:{ex.Body.Code} message:{ex.Body.Message}");
                }
            }
            else if (this.ParameterSetName.Equals(Constants.ParameterSetNameConvertLegacyPeering))
            {
                try
                {
                    return (PSPeering)this.PutNewPeering(this.ConvertClassicToDirectPeering(this.LegacyPeering));
                }
                catch (ErrorResponseException ex)
                {
                    throw new Exception($"Error:{ex.Response.ReasonPhrase} reason:{ex.Body.Code} message:{ex.Body.Message}");
                }
            }
            else
            {
                throw new PSArgumentException($"PSPeering {this.ParameterSetName} is not recognized Try Again");
            }
        }

        /// <summary>
        /// The create exchange peering.
        /// </summary>
        /// <returns>
        /// The <see cref="PSPeering"/>.
        /// </returns>
        /// <exception cref="PSArgumentNullException">
        /// </exception>
        /// <exception cref="PSArgumentException">
        /// </exception>
        /// <exception cref="Exception">
        /// </exception>
        /// <exception cref="ArmErrorException">
        /// </exception>
        /// <exception cref="HttpRequestException">
        /// </exception>
        private PSPeering CreateExchangePeering()
        {

            var peeringRequest =
                new PSPeering(
                    name: this.Name,
                    location: this.GetAzureRegion(this.PeeringLocation, Constants.Exchange),
                    sku: new PSPeeringSku { Name = "basic_Exchange_Free" },
                    kind: Constants.Exchange)
                {
                    PeeringLocation = this.PeeringLocation,
                    Exchange = new PSPeeringPropertiesExchange
                    {
                        Connections = new List<PSExchangeConnection>(),
                        PeerAsn = new PSSubResource(this.PeerAsnResourceId)
                    },
                    Tags = this.ConvertToDictionary(this.Tag),
                };
            if (this.ExchangeConnection == null)
            {
                throw new PSArgumentNullException($"Exchange connection Cannot be null. Create a new PSExchangePeeringConnection by using the New-AzExchangePeeringConnection cmdlet.");
            }

            foreach (PSExchangeConnection psExchangeConnection in this.ExchangeConnection)
            {
                if (this.ValidConnection(psExchangeConnection))
                    peeringRequest.Exchange.Connections.Add(psExchangeConnection);
            }

            if (this.ParameterSetName.Equals(Constants.Exchange))
            {
                if (peeringRequest.Exchange?.Connections.Count <= 0)
                    throw new PSArgumentException($"Exchange Connection cannot be is null.");
                if (peeringRequest.Direct != null)
                {
                    peeringRequest.Direct = null;
                }
                try
                {
                    return (PSPeering)this.PutNewPeering(peeringRequest);
                }
                catch (ErrorResponseException ex)
                {
                    throw new Exception($"Error:{ex.Response.ReasonPhrase} reason:{ex.Body.Code} message:{ex.Body.Message}");
                }
            }
            else if (this.ParameterSetName.Equals(Constants.ParameterSetNameConvertLegacyPeering))
            {
                try
                {
                    return (PSPeering)this.PutNewPeering(this.ConvertClassicToExchangePeering(this.LegacyPeering));
                }
                catch (ErrorResponseException ex)
                {
                    throw new Exception($"Error:{ex.Response.ReasonPhrase} reason:{ex.Body.Code} message:{ex.Body.Message}");
                }
            }
            else
            {
                throw new PSArgumentException($"PSPeering {this.ParameterSetName} is not recognized Try Again");
            }
        }

        /// <summary>
        /// The put new InputObject.
        /// </summary>
        /// <param name="newPeering">
        /// The new InputObject.
        /// </param>
        /// <returns>
        /// The <see cref="object"/>.
        /// </returns>
        private object PutNewPeering(PSPeering newPeering)
        {
            var peering = PeeringResourceManagerProfile.Mapper.Map<PeeringModel>(newPeering);
            this.PeeringClient.CreateOrUpdate(
                this.ResourceGroupName,
                this.Name,
                peering);
            return PeeringResourceManagerProfile.Mapper.Map<PSPeering>(
                this.PeeringClient.Get(this.ResourceGroupName, this.Name));
        }

        /// <summary>
        /// The convert classic to Exchange peering.
        /// </summary>
        /// <param name="classicPeering">
        /// The classic peering.
        /// </param>
        /// <returns>
        /// The <see cref="object"/>.
        /// </returns>
        private PSPeering ConvertClassicToExchangePeering(PSPeering classicPeering)
        {
            if (classicPeering == null)
                throw new ArgumentNullException("Unable to convert legacy peering because value was null or empty.");
            if (classicPeering.Exchange.Connections == null)
                throw new ArgumentNullException("Unable to convert legacy peering.");
            var connections = new List<PSExchangeConnection>();
            foreach (var sessionConfiguration in classicPeering.Exchange.Connections)
            {
                connections.Add(sessionConfiguration);
            }

            var newPeering = new PSPeering
            {
                Location = this.GetAzureRegion(this.PeeringLocation, Constants.Exchange),
                PeeringLocation =
                                              classicPeering.PeeringLocation ?? this.PeeringLocation,
                Kind = classicPeering.Kind ?? Constants.Exchange,
                Sku =
                                              classicPeering.Sku
                                              ?? new PSPeeringSku { Name = Constants.BasicExchangeFree },
                Exchange = new PSPeeringPropertiesExchange
                {
                    Connections = classicPeering.Exchange.Connections,
                    PeerAsn = new PSSubResource(this.PeerAsnResourceId)
                }
            };

            // set default max prefixes 
            foreach (var connection in newPeering.Exchange.Connections)
            {
                connection.BgpSession.MaxPrefixesAdvertisedV4 = connection.BgpSession.MaxPrefixesAdvertisedV4 != 0 ? connection.BgpSession.MaxPrefixesAdvertisedV4 : 20000;
                connection.BgpSession.MaxPrefixesAdvertisedV6 = connection.BgpSession.MaxPrefixesAdvertisedV6 != 0 ? connection.BgpSession.MaxPrefixesAdvertisedV6 : 2000;
            }

            return newPeering;
        }

        /// <summary>
        /// The convert classic to direct peering.
        /// </summary>
        /// <param name="classicPeering">
        /// The classic peering.
        /// </param>
        /// <returns>
        /// The <see cref="object"/>.
        /// </returns>
        private PSPeering ConvertClassicToDirectPeering(PSPeering classicPeering)
        {
            if (classicPeering == null)
                throw new ArgumentNullException("Unable to convert legacy peering because value was null or empty.");
            if (classicPeering.Direct.Connections == null)
                throw new ArgumentNullException("Unable to convert legacy peering.");
            var connections = new List<PSDirectConnection>();
            foreach (var sessionConfiguration in classicPeering.Direct.Connections)
            {
                connections.Add(sessionConfiguration);
            }

            var newPeering = new PSPeering
                                 {
                                     Location = this.GetAzureRegion(this.PeeringLocation, Constants.Direct),
                                     PeeringLocation = classicPeering.PeeringLocation ?? this.PeeringLocation,
                                     Kind = classicPeering.Kind ?? Constants.Direct,
                                     Sku = classicPeering.Sku ?? new PSPeeringSku(Constants.BasicDirectFree),
                                     Direct = new PSPeeringPropertiesDirect
                                                  {
                                                      Connections = classicPeering.Direct.Connections,
                                                      PeerAsn = new PSSubResource(this.PeerAsnResourceId)
                                                  }
                                 };
            foreach (var connection in newPeering.Direct.Connections)
            {
                connection.BandwidthInMbps = connection.ProvisionedBandwidthInMbps ?? 10000;
                connection.BgpSession.MaxPrefixesAdvertisedV4  = connection.BgpSession.MaxPrefixesAdvertisedV4 != 0 ? connection.BgpSession.MaxPrefixesAdvertisedV4 : 20000;
                connection.BgpSession.MaxPrefixesAdvertisedV6 = connection.BgpSession.MaxPrefixesAdvertisedV6 != 0 ? connection.BgpSession.MaxPrefixesAdvertisedV6 : 2000;
            }

            return newPeering;
        }
    }
}
