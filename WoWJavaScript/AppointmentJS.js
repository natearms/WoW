function openRaidEvent(context) {
    var onLoadContext = context.getFormContext();;

    var raidEvent = Xrm.Page.getAttribute("regardingobjectid").getValue();
    if (raidEvent[0].id != null) {

        var raidEventGuid = raidEvent[0].id.replace("{", "").replace("}", "");
        var url = "https://thehouse.crm.dynamics.com/main.aspx?etc=10086&pagetype=entityrecord&id=%7B" + raidEventGuid + "%7D";

        //onLoadContext.ui.close();
        Xrm.Navigation.openUrl(url);
    }
}