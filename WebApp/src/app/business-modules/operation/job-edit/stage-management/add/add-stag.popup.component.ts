import { Component, OnInit } from "@angular/core";
import { CdkDragDrop, moveItemInArray } from "@angular/cdk/drag-drop";

@Component({
  selector: "add-stag-popup",
  templateUrl: "./add-stage.popup.component.html",
  styleUrls: ["./add-stage.popup.cmponent.scss"]
})
export class OpsModuleStageManagementAddStagePopupComponent implements OnInit {
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

  constructor() {}

  ngOnInit() {}

  dropped(event: CdkDragDrop<any[]>) {
    moveItemInArray(this.numbers, event.previousIndex, event.currentIndex);
  }
}
