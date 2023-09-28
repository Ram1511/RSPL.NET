using System.ComponentModel;

namespace RSPL.Common
{
    public class Enums
    {
        public enum BrandName
        {
            RSPL,
            [Description("RSPL-BD")]
            RSPLBD,
            Competitor
        }
        public enum ComplaintSource
        {
            [Description("Consumer")]
            Consumer,
            [Description("Business Partner")]
            BusinessPartner,
            [Description("Former Business Partner")]
            FormerBusinessPartner,
            [Description("Consumer Or Business Partner")]
            ConsumerOrBusinessPartner,
            Retailer
        }
        //rspl-bd  retailer enabled
        public enum SampleSource
        {
            [Description("Business Partner")]
            BusinessPartner,
            [Description("Former Business Partner")]
            FormerBusinessPartner,
            [Description("RSPL-BD")]
            RSPLBD,
            Retailer,
            FinishedGoods
        }
        public enum SampleTakenFrom
        {
            Godown,
            Vehicle
        }
        public enum Category
        {
            [Description("Finished Goods")]
            FinishedGoods,
            [Description("Semi Finished Goods")]
            SemiFinishedGoods,
            [Description("Raw Material")]
            RawMaterial

        }
        public enum TestType
        {
            Quantitative,
            Qualitative
        }
        public enum QualitativeValue
        {
            Ok,
            [Description("Not Ok")]
            NotOk
        }
        public enum MarketSampleType
        {
            [Description("Quality Complaint")]
            QualityComplaint,
            [Description("Market Sample")]
            MarketSample
        }

        public enum ProductType
        {
            Detergent,
            Cake,
            Material
        }

        public enum TestCategory
        {
            Regular,
            Complete,
            Routine
        }
        public enum TestResult
        {
            Pass,
            Failed,
            NA
        }
        public enum SampleReceivedFrom
        {
            Factory,
            Market
        }
        public enum Shift
        {
            Day,
            Night
        }
        public enum BrandType
        {
            [Description("Own Brand")]
            OwnBrand,
            [Description("Competitor Brand")]
            CompetitorBrand
        }
        public enum SourceType
        {
            Factory,
            Market,
            QualityComplaint
        }
        public enum StateMaster
        {
            [Description("JAMMU AND KASHMIR")]
            JAMMUANDKASHMIR,
            [Description("HIMACHAL PRADESH")]
            HIMACHALPRADESH,
            [Description("PUNJAB")]
            PUNJAB,
            [Description("CHANDIGARH")]
            CHANDIGARH,
            [Description("UTTARAKHAND")]
            UTTARAKHAND,
            [Description("HARYANA")]
            HARYANA,
            [Description("DELHI")]
            DELHI,
            [Description("RAJASTHAN")]
            RAJASTHAN,
            [Description("UTTAR PRADESH")]
            UTTARPRADESH,
            [Description("BIHAR")]
            BIHAR,
            [Description("SIKKIM")]
            SIKKIM,
            [Description("ARUNACHAL PRADESH")]
            ARUNACHALPRADESH,
            [Description("NAGALAND")]
            NAGALAND,
            [Description("MANIPUR")]
            MANIPUR,
            [Description("MIZORAM")]
            MIZORAM,
            [Description("TRIPURA")]
            TRIPURA,
            [Description("MEGHALAYA")]
            MEGHALAYA,
            [Description("ASSAM")]
            ASSAM,
            [Description("WEST BENGAL")]
            WESTBENGAL,
            [Description("JHARKHAND")]
            JHARKHAND,
            [Description("ODISHA")]
            ODISHA,
            [Description("CHHATTISGARH")]
            CHHATTISGARH,
            [Description("MADHYA PRADESH")]
            MADHYAPRADESH,
            [Description("GUJARAT")]
            GUJARAT,
            [Description("DAMAN AND DIU")]
            DAMANANDDIU,
            [Description("DADRA AND NAGAR HAVELI")]
            DADRAANDNAGARHAVELI,
            [Description("MAHARASHTRA")]
            MAHARASHTRA,
            [Description("KARNATAKA")]
            KARNATAKA,
            [Description("GOA")]
            GOA,
            [Description("LAKSHADWEEP")]
            LAKSHADWEEP,
            [Description("KERALA")]
            KERALA,
            [Description("TAMILNADU")]
            TAMILNADU,
            [Description("PUDUCHERRY")]
            PUDUCHERRY,
            [Description("ANDAMAN AND NICOBAR")]
            ANDAMANANDNICOBAR,
            [Description("TELANGANA")]
            TELANGANA,
            [Description("ANDHRA PRADESH")]
            ANDHRAPRADESH,
            [Description("LADAKH")]
            LADAKH
        }
    }
}