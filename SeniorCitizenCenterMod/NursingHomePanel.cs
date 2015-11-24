namespace SeniorCitizenCenterMod {
    public class NursingHomePanel : CustomBasePanel {

        protected override bool IsServiceValid(BuildingInfo info) {
            // Service is only valid for Healthcare Buildings with the NursingHomeAi
            return info != null && info.m_buildingAI is NursingHomeAi;
        }

    }
}