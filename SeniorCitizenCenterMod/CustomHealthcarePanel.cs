namespace SeniorCitizenCenterMod {
    public class CustomHealthcarePanel : CustomBasePanel {

        protected override bool IsServiceValid(BuildingInfo info) {
            // Service is only valid for Healthcare Buildings without the NursingHomeAi
            return info != null && info.GetService() == this.service && !(info.m_buildingAI is NursingHomeAi);
        }

    }
}