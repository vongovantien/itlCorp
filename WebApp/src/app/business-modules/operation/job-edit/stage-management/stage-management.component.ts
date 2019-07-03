import { Component, OnInit, ViewChild } from "@angular/core";
import { AppPage } from "src/app/app.base";
import { OpsModuleStageManagementAddStagePopupComponent } from "./add/add-stage.popup.component";

@Component({
    selector: "app-ops-module-stage-management",
    templateUrl: "./stage-management.component.html",
    styleUrls: ["./stage-management.component.scss"]
})
export class OpsModuleStageManagementComponent extends AppPage {
    @ViewChild(OpsModuleStageManagementAddStagePopupComponent, {
        static: false
    })
    popupCreate: OpsModuleStageManagementAddStagePopupComponent;
    constructor() {
        super();
    }

    openPopUpCreateStage() {
        this.popupCreate.show();
    }
}
