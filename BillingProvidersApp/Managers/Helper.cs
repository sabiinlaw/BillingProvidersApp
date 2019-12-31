using SuiteTalk;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace BillingProvidersApp.Managers
{
    public static class Helper
    {
        const string DefaultLeadStatusName = "Unread";
        const string DefaultCustomerStatusName = "Closed Won";
        const string DetailMessageSeparator = ";";

        const string DefaultPriceLevelSetting = "default-pricelevel-name";
        const string DefaultLeadSourceSetting = "default-leadsource-id";

        const string SalesTaxItemIdSetting = "sales-tax-item-id";

        const string WonStatusEntityCustomFieldIdSetting = "won-status-entity-custom-field-id";
        const string WonStatusValueSetting = "won-status-field-value";

        const string CrmRoleEntityCustomFieldIdSetting = "crm-role-entity-custom-field-id";
        const string CrmRoleValueSetting = "crm-role-field-value";

        public const string WonSubscriptionTypeCustomFieldIdSetting = "won_subscription_type-entity-custom-field-id";

        const string WonPrimaryRequestorIdSetting = "won-primary-requestor-id";

        const string DefaultDiscountTermIdSetting = "default-discount-term-id";

        const string DefaultAccountReceivableIDSetting = "A/R-account";

        const string CarrierSalesRepFieldIdSetting = "customer-carrier-sales-rep-field-id";
        const string CarrierSalesRepEmailFieldIdSetting = "customer-carrier-sales-rep-email-field-id";
        const string CarrierSalesRepPhoneFieldIdSetting = "customer-carrier-sales-rep-phone-field-id";
        const string DescriptionFieldIdSetting = "customer-description-field-id";

        const string BillingCustomerStatusNameSetting = "billing-customer-status-name";

        const string EftPaymentMethodNameSetting = "eft-payment-method";

        const string EcosystemSalesRepEmployeeIdSetting = "ecosystem-salesrep-empl-id";
        const string CorrigoProductIfsmIdSetting = "corrigo-product-ifsm-id";

        const string LeadString = "lead";
        const string ProspectString = "prospect";
        const string CustomerString = "customer";

        const int NetSuitePhoneMaxLength = 21;

        private static Dictionary<string, Country> _countryCode2nsCountryMap;

        // Cached data
        private static ConcurrentDictionary<Tuple<CustomerStatusStage, string>, CustomerStatus> _customerStatuses = new ConcurrentDictionary<Tuple<CustomerStatusStage, string>, CustomerStatus>();
        private static ConcurrentDictionary<string, CustomerStatus> _customerStatusesByInternalIds = new ConcurrentDictionary<string, CustomerStatus>();
        private static PriceLevel _defaultPriceLevel = null;
        private static ConcurrentDictionary<string, PaymentMethod> _paymentMethods = new ConcurrentDictionary<string, PaymentMethod>();
        private static ConcurrentDictionary<string, SalesTaxItem> _salesTaxItems = new ConcurrentDictionary<string, SalesTaxItem>();

        static Helper()
        {
            #region initNSMapping

            _countryCode2nsCountryMap = new Dictionary<string, Country>();
            _countryCode2nsCountryMap.Add("AF", Country._afghanistan);
            _countryCode2nsCountryMap.Add("AX", Country._alandIslands);
            _countryCode2nsCountryMap.Add("AL", Country._albania);
            _countryCode2nsCountryMap.Add("DZ", Country._algeria);
            _countryCode2nsCountryMap.Add("AS", Country._americanSamoa);
            _countryCode2nsCountryMap.Add("AD", Country._andorra);
            _countryCode2nsCountryMap.Add("AO", Country._angola);
            _countryCode2nsCountryMap.Add("AI", Country._anguilla);
            _countryCode2nsCountryMap.Add("AQ", Country._antarctica);
            _countryCode2nsCountryMap.Add("AG", Country._antiguaAndBarbuda);
            _countryCode2nsCountryMap.Add("AR", Country._argentina);
            _countryCode2nsCountryMap.Add("AM", Country._armenia);
            _countryCode2nsCountryMap.Add("AW", Country._aruba);
            _countryCode2nsCountryMap.Add("AU", Country._australia);
            _countryCode2nsCountryMap.Add("AT", Country._austria);
            _countryCode2nsCountryMap.Add("AZ", Country._azerbaijan);
            _countryCode2nsCountryMap.Add("BS", Country._bahamas);
            _countryCode2nsCountryMap.Add("BH", Country._bahrain);
            _countryCode2nsCountryMap.Add("BD", Country._bangladesh);
            _countryCode2nsCountryMap.Add("BB", Country._barbados);
            _countryCode2nsCountryMap.Add("BY", Country._belarus);
            _countryCode2nsCountryMap.Add("BE", Country._belgium);
            _countryCode2nsCountryMap.Add("BZ", Country._belize);
            _countryCode2nsCountryMap.Add("BJ", Country._benin);
            _countryCode2nsCountryMap.Add("BM", Country._bermuda);
            _countryCode2nsCountryMap.Add("BT", Country._bhutan);
            _countryCode2nsCountryMap.Add("BO", Country._bolivia);
            _countryCode2nsCountryMap.Add("BA", Country._bonaireSaintEustatiusAndSaba);
            _countryCode2nsCountryMap.Add("BW", Country._bosniaAndHerzegovina);
            _countryCode2nsCountryMap.Add("BV", Country._botswana);
            _countryCode2nsCountryMap.Add("BR", Country._bouvetIsland);
            _countryCode2nsCountryMap.Add("VG", Country._brazil);
            _countryCode2nsCountryMap.Add("IO", Country._britishIndianOceanTerritory);
            _countryCode2nsCountryMap.Add("BN", Country._bruneiDarussalam);
            _countryCode2nsCountryMap.Add("BG", Country._bulgaria);
            _countryCode2nsCountryMap.Add("BF", Country._burkinaFaso);
            _countryCode2nsCountryMap.Add("BI", Country._burundi);
            _countryCode2nsCountryMap.Add("KH", Country._cambodia);
            _countryCode2nsCountryMap.Add("CM", Country._cameroon);
            _countryCode2nsCountryMap.Add("CA", Country._canada);
            _countryCode2nsCountryMap.Add("CV", Country._capeVerde);
            _countryCode2nsCountryMap.Add("KY", Country._caymanIslands);
            _countryCode2nsCountryMap.Add("CF", Country._centralAfricanRepublic);
            _countryCode2nsCountryMap.Add("TD", Country._ceutaAndMelilla);
            _countryCode2nsCountryMap.Add("CL", Country._chad);
            _countryCode2nsCountryMap.Add("CN", Country._chile);
            _countryCode2nsCountryMap.Add("HK", Country._china);
            _countryCode2nsCountryMap.Add("CX", Country._christmasIsland);
            _countryCode2nsCountryMap.Add("CC", Country._cocosKeelingIslands);
            _countryCode2nsCountryMap.Add("CO", Country._colombia);
            _countryCode2nsCountryMap.Add("KM", Country._comoros);
            _countryCode2nsCountryMap.Add("CG", Country._congoDemocraticPeoplesRepublic);
            _countryCode2nsCountryMap.Add("CD", Country._congoRepublicOf);
            _countryCode2nsCountryMap.Add("CK", Country._cookIslands);
            _countryCode2nsCountryMap.Add("CR", Country._costaRica);
            _countryCode2nsCountryMap.Add("CI", Country._coteDIvoire);
            _countryCode2nsCountryMap.Add("HR", Country._croatiaHrvatska);
            _countryCode2nsCountryMap.Add("CU", Country._cuba);
            _countryCode2nsCountryMap.Add("CY", Country._cyprus);
            _countryCode2nsCountryMap.Add("CZ", Country._czechRepublic);
            _countryCode2nsCountryMap.Add("DK", Country._denmark);
            _countryCode2nsCountryMap.Add("DJ", Country._djibouti);
            _countryCode2nsCountryMap.Add("DM", Country._dominica);
            _countryCode2nsCountryMap.Add("DO", Country._dominicanRepublic);
            _countryCode2nsCountryMap.Add("EC", Country._ecuador);
            _countryCode2nsCountryMap.Add("EG", Country._egypt);
            _countryCode2nsCountryMap.Add("SV", Country._elSalvador);
            _countryCode2nsCountryMap.Add("GQ", Country._equatorialGuinea);
            _countryCode2nsCountryMap.Add("ER", Country._eritrea);
            _countryCode2nsCountryMap.Add("EE", Country._estonia);
            _countryCode2nsCountryMap.Add("ET", Country._ethiopia);
            _countryCode2nsCountryMap.Add("FK", Country._falklandIslands);
            _countryCode2nsCountryMap.Add("FO", Country._faroeIslands);
            _countryCode2nsCountryMap.Add("FJ", Country._fiji);
            _countryCode2nsCountryMap.Add("FI", Country._finland);
            _countryCode2nsCountryMap.Add("FR", Country._france);
            _countryCode2nsCountryMap.Add("GF", Country._frenchGuiana);
            _countryCode2nsCountryMap.Add("PF", Country._frenchPolynesia);
            _countryCode2nsCountryMap.Add("TF", Country._frenchSouthernTerritories);
            _countryCode2nsCountryMap.Add("GA", Country._gabon);
            _countryCode2nsCountryMap.Add("GM", Country._gambia);
            _countryCode2nsCountryMap.Add("GE", Country._georgia);
            _countryCode2nsCountryMap.Add("DE", Country._germany);
            _countryCode2nsCountryMap.Add("GH", Country._ghana);
            _countryCode2nsCountryMap.Add("GI", Country._gibraltar);
            _countryCode2nsCountryMap.Add("GR", Country._greece);
            _countryCode2nsCountryMap.Add("GL", Country._greenland);
            _countryCode2nsCountryMap.Add("GD", Country._grenada);
            _countryCode2nsCountryMap.Add("GP", Country._guadeloupe);
            _countryCode2nsCountryMap.Add("GU", Country._guam);
            _countryCode2nsCountryMap.Add("GT", Country._guatemala);
            _countryCode2nsCountryMap.Add("GG", Country._guernsey);
            _countryCode2nsCountryMap.Add("GN", Country._guinea);
            _countryCode2nsCountryMap.Add("GW", Country._guineaBissau);
            _countryCode2nsCountryMap.Add("GY", Country._guyana);
            _countryCode2nsCountryMap.Add("HT", Country._haiti);
            _countryCode2nsCountryMap.Add("HM", Country._heardAndMcDonaldIslands);
            _countryCode2nsCountryMap.Add("VA", Country._holySeeCityVaticanState);
            _countryCode2nsCountryMap.Add("HN", Country._honduras);
            _countryCode2nsCountryMap.Add("HU", Country._hungary);
            _countryCode2nsCountryMap.Add("IS", Country._iceland);
            _countryCode2nsCountryMap.Add("IN", Country._india);
            _countryCode2nsCountryMap.Add("ID", Country._indonesia);
            _countryCode2nsCountryMap.Add("IR", Country._iranIslamicRepublicOf);
            _countryCode2nsCountryMap.Add("IQ", Country._iraq);
            _countryCode2nsCountryMap.Add("IE", Country._ireland);
            _countryCode2nsCountryMap.Add("IM", Country._isleOfMan);
            _countryCode2nsCountryMap.Add("IL", Country._israel);
            _countryCode2nsCountryMap.Add("IT", Country._italy);
            _countryCode2nsCountryMap.Add("JM", Country._jamaica);
            _countryCode2nsCountryMap.Add("JP", Country._japan);
            _countryCode2nsCountryMap.Add("JE", Country._jersey);
            _countryCode2nsCountryMap.Add("JO", Country._jordan);
            _countryCode2nsCountryMap.Add("KZ", Country._kazakhstan);
            _countryCode2nsCountryMap.Add("KE", Country._kenya);
            _countryCode2nsCountryMap.Add("KI", Country._kiribati);
            _countryCode2nsCountryMap.Add("KP", Country._koreaDemocraticPeoplesRepublic);
            _countryCode2nsCountryMap.Add("KR", Country._koreaRepublicOf);
            _countryCode2nsCountryMap.Add("KW", Country._kuwait);
            _countryCode2nsCountryMap.Add("KG", Country._kyrgyzstan);
            _countryCode2nsCountryMap.Add("LA", Country._laoPeoplesDemocraticRepublic);
            _countryCode2nsCountryMap.Add("LV", Country._latvia);
            _countryCode2nsCountryMap.Add("LB", Country._lebanon);
            _countryCode2nsCountryMap.Add("LS", Country._lesotho);
            _countryCode2nsCountryMap.Add("LR", Country._liberia);
            _countryCode2nsCountryMap.Add("LY", Country._libya);
            _countryCode2nsCountryMap.Add("LI", Country._liechtenstein);
            _countryCode2nsCountryMap.Add("LT", Country._lithuania);
            _countryCode2nsCountryMap.Add("LU", Country._luxembourg);
            _countryCode2nsCountryMap.Add("MK", Country._macedonia);
            _countryCode2nsCountryMap.Add("MG", Country._madagascar);
            _countryCode2nsCountryMap.Add("MW", Country._malawi);
            _countryCode2nsCountryMap.Add("MY", Country._malaysia);
            _countryCode2nsCountryMap.Add("MV", Country._maldives);
            _countryCode2nsCountryMap.Add("ML", Country._mali);
            _countryCode2nsCountryMap.Add("MT", Country._malta);
            _countryCode2nsCountryMap.Add("MH", Country._marshallIslands);
            _countryCode2nsCountryMap.Add("MQ", Country._martinique);
            _countryCode2nsCountryMap.Add("MR", Country._mauritania);
            _countryCode2nsCountryMap.Add("MU", Country._mauritius);
            _countryCode2nsCountryMap.Add("YT", Country._mayotte);
            _countryCode2nsCountryMap.Add("MX", Country._mexico);
            _countryCode2nsCountryMap.Add("FM", Country._micronesiaFederalStateOf);
            _countryCode2nsCountryMap.Add("MD", Country._moldovaRepublicOf);
            _countryCode2nsCountryMap.Add("MC", Country._monaco);
            _countryCode2nsCountryMap.Add("MN", Country._mongolia);
            _countryCode2nsCountryMap.Add("ME", Country._montenegro);
            _countryCode2nsCountryMap.Add("MS", Country._montserrat);
            _countryCode2nsCountryMap.Add("MA", Country._morocco);
            _countryCode2nsCountryMap.Add("MZ", Country._mozambique);
            _countryCode2nsCountryMap.Add("MM", Country._myanmar);
            _countryCode2nsCountryMap.Add("NA", Country._namibia);
            _countryCode2nsCountryMap.Add("NR", Country._nauru);
            _countryCode2nsCountryMap.Add("NP", Country._nepal);
            _countryCode2nsCountryMap.Add("NL", Country._netherlands);
            _countryCode2nsCountryMap.Add("NC", Country._newCaledonia);
            _countryCode2nsCountryMap.Add("NZ", Country._newZealand);
            _countryCode2nsCountryMap.Add("NI", Country._nicaragua);
            _countryCode2nsCountryMap.Add("NE", Country._niger);
            _countryCode2nsCountryMap.Add("NG", Country._nigeria);
            _countryCode2nsCountryMap.Add("NU", Country._niue);
            _countryCode2nsCountryMap.Add("NF", Country._norfolkIsland);
            _countryCode2nsCountryMap.Add("MP", Country._northernMarianaIslands);
            _countryCode2nsCountryMap.Add("NO", Country._norway);
            _countryCode2nsCountryMap.Add("OM", Country._oman);
            _countryCode2nsCountryMap.Add("PK", Country._pakistan);
            _countryCode2nsCountryMap.Add("PW", Country._palau);
            _countryCode2nsCountryMap.Add("PS", Country._stateOfPalestine);
            _countryCode2nsCountryMap.Add("PA", Country._panama);
            _countryCode2nsCountryMap.Add("PG", Country._papuaNewGuinea);
            _countryCode2nsCountryMap.Add("PY", Country._paraguay);
            _countryCode2nsCountryMap.Add("PE", Country._peru);
            _countryCode2nsCountryMap.Add("PH", Country._philippines);
            _countryCode2nsCountryMap.Add("PN", Country._pitcairnIsland);
            _countryCode2nsCountryMap.Add("PL", Country._poland);
            _countryCode2nsCountryMap.Add("PT", Country._portugal);
            _countryCode2nsCountryMap.Add("PR", Country._puertoRico);
            _countryCode2nsCountryMap.Add("QA", Country._qatar);
            _countryCode2nsCountryMap.Add("RE", Country._reunionIsland);
            _countryCode2nsCountryMap.Add("RO", Country._romania);
            _countryCode2nsCountryMap.Add("RU", Country._russianFederation);
            _countryCode2nsCountryMap.Add("RW", Country._rwanda);
            _countryCode2nsCountryMap.Add("BL", Country._saintBarthelemy);
            _countryCode2nsCountryMap.Add("SH", Country._saintHelena);
            _countryCode2nsCountryMap.Add("KN", Country._saintKittsAndNevis);
            _countryCode2nsCountryMap.Add("LC", Country._saintLucia);
            _countryCode2nsCountryMap.Add("MF", Country._saintMartin);
            _countryCode2nsCountryMap.Add("VC", Country._saintVincentAndTheGrenadines);
            _countryCode2nsCountryMap.Add("WS", Country._samoa);
            _countryCode2nsCountryMap.Add("SM", Country._sanMarino);
            _countryCode2nsCountryMap.Add("ST", Country._saoTomeAndPrincipe);
            _countryCode2nsCountryMap.Add("SA", Country._saudiArabia);
            _countryCode2nsCountryMap.Add("SN", Country._senegal);
            _countryCode2nsCountryMap.Add("RS", Country._serbia);
            _countryCode2nsCountryMap.Add("SC", Country._seychelles);
            _countryCode2nsCountryMap.Add("SL", Country._sierraLeone);
            _countryCode2nsCountryMap.Add("SG", Country._singapore);
            _countryCode2nsCountryMap.Add("SK", Country._slovakRepublic);
            _countryCode2nsCountryMap.Add("SI", Country._slovenia);
            _countryCode2nsCountryMap.Add("SB", Country._solomonIslands);
            _countryCode2nsCountryMap.Add("SO", Country._somalia);
            _countryCode2nsCountryMap.Add("ZA", Country._southAfrica);
            _countryCode2nsCountryMap.Add("GS", Country._southGeorgia);
            _countryCode2nsCountryMap.Add("SS", Country._southSudan);
            _countryCode2nsCountryMap.Add("ES", Country._spain);
            _countryCode2nsCountryMap.Add("LK", Country._sriLanka);
            _countryCode2nsCountryMap.Add("SD", Country._sudan);
            _countryCode2nsCountryMap.Add("SR", Country._suriname);
            _countryCode2nsCountryMap.Add("SJ", Country._svalbardAndJanMayenIslands);
            _countryCode2nsCountryMap.Add("SZ", Country._swaziland);
            _countryCode2nsCountryMap.Add("SE", Country._sweden);
            _countryCode2nsCountryMap.Add("CH", Country._switzerland);
            _countryCode2nsCountryMap.Add("SY", Country._syrianArabRepublic);
            _countryCode2nsCountryMap.Add("TW", Country._taiwan);
            _countryCode2nsCountryMap.Add("TJ", Country._tajikistan);
            _countryCode2nsCountryMap.Add("TZ", Country._tanzania);
            _countryCode2nsCountryMap.Add("TH", Country._thailand);
            _countryCode2nsCountryMap.Add("TG", Country._togo);
            _countryCode2nsCountryMap.Add("TK", Country._tokelau);
            _countryCode2nsCountryMap.Add("TO", Country._tonga);
            _countryCode2nsCountryMap.Add("TT", Country._trinidadAndTobago);
            _countryCode2nsCountryMap.Add("TN", Country._tunisia);
            _countryCode2nsCountryMap.Add("TR", Country._turkey);
            _countryCode2nsCountryMap.Add("TM", Country._turkmenistan);
            _countryCode2nsCountryMap.Add("TC", Country._turksAndCaicosIslands);
            _countryCode2nsCountryMap.Add("TV", Country._tuvalu);
            _countryCode2nsCountryMap.Add("UG", Country._uganda);
            _countryCode2nsCountryMap.Add("UA", Country._ukraine);
            _countryCode2nsCountryMap.Add("AE", Country._unitedArabEmirates);
            _countryCode2nsCountryMap.Add("GB", Country._unitedKingdom);
            _countryCode2nsCountryMap.Add("US", Country._unitedStates);
            _countryCode2nsCountryMap.Add("UY", Country._uruguay);
            _countryCode2nsCountryMap.Add("UZ", Country._uzbekistan);
            _countryCode2nsCountryMap.Add("VU", Country._vanuatu);
            _countryCode2nsCountryMap.Add("VE", Country._venezuela);
            _countryCode2nsCountryMap.Add("VN", Country._vietnam);
            _countryCode2nsCountryMap.Add("VI", Country._virginIslandsBritish);
            _countryCode2nsCountryMap.Add("WF", Country._virginIslandsUSA);
            _countryCode2nsCountryMap.Add("EH", Country._westernSahara);
            _countryCode2nsCountryMap.Add("YE", Country._yemen);
            _countryCode2nsCountryMap.Add("ZM", Country._zambia);
            _countryCode2nsCountryMap.Add("ZW", Country._zimbabwe);

            #endregion
        }

        public static string GetWonInvoiceInstanceFieldId()
        {
            return "custbody16";
        }
        public static CustomerCreditCards GetDefaultOrFirstCustomerCreditCards(CustomerCreditCardsList creditCardList)
        {
            if (creditCardList == null || creditCardList.creditCards == null ||
                 creditCardList.creditCards.Length == 0)
                return null;

            /// Try to search the default credit card
            foreach (CustomerCreditCards creditCard in creditCardList.creditCards)
            {
                if (creditCard.ccDefaultSpecified && creditCard.ccDefault && !Helper.IsCredirCardExpired(creditCard))
                    return creditCard;
            }

            /// If the default credit card is not found then first non expired credit card is used
            foreach (CustomerCreditCards creditCard in creditCardList.creditCards)
            {
                if (!IsCredirCardExpired(creditCard))
                    return creditCard;
            }

            return null;
        }

        public static string GetDefaultAccountReceivableId()
        {
            return DefaultAccountReceivableIDSetting;
        }

        public static string GetDefaultDiscountTermId()
        {
            return DefaultDiscountTermIdSetting;
        }
        public static RecordRef CreateRecordRefWithId(string internalId)
        {
            if (internalId == null || internalId.Trim().Length == 0)
                return null;

            RecordRef recordRef = new RecordRef();
            recordRef.internalId = internalId;

            return recordRef;
        }

        /// <summary>
        /// Checks if the Customer CreditCard expired
        /// </summary>
        public static bool IsCredirCardExpired(CustomerCreditCards creditCard)
        {
            if (null == creditCard)
                throw new ArgumentNullException("creditCard");

            DateTime today = DateTime.Today;

            return !(creditCard.ccExpireDateSpecified &&
                ((creditCard.ccExpireDate.Year > today.Year) ||
                ((creditCard.ccExpireDate.Year == today.Year) && (creditCard.ccExpireDate.Month >= today.Month))));
        }

    }
}
