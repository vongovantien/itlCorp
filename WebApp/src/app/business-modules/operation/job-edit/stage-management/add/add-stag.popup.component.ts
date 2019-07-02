import { Component, OnInit } from "@angular/core";
import { CdkDragDrop, moveItemInArray } from "@angular/cdk/drag-drop";
import { PopupBase } from "src/app/modal.base";

@Component({
    selector: "add-stage-popup",
    templateUrl: "./add-stage.popup.component.html",
    styleUrls: ["./add-stage.popup.component.scss"]
})
export class OpsModuleStageManagementAddStagePopupComponent extends PopupBase {
    numbers: any[] = [
        {
            name: "thor",
            id: 1
        },
        {
            name: "lee",
            id: 2
        },
        {
            name: "tam",
            id: 3
        },
        {
            name: "hau mon",
            id: 4
        }
    ];

    constructor() {
        super();
    }

    dropped(event: CdkDragDrop<any[]>) {
        moveItemInArray(this.numbers, event.previousIndex, event.currentIndex);
    }
}
