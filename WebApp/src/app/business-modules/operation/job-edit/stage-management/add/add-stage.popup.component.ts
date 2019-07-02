import { Component, OnInit, OnChanges } from "@angular/core";
import { PopupBase } from "src/app/modal.base";

import * as _ from "lodash";

@Component({
  selector: "add-stage-popup",
  templateUrl: "./add-stage.popup.component.html",
  styleUrls: ["./add-stage.popup.component.scss"]
})
export class OpsModuleStageManagementAddStagePopupComponent extends PopupBase implements OnInit, OnChanges {
  stages: any[] = [
    {
      name: "Nộp chứng từ",
      code: "AAAA1111",
      id: 1
    },
    {
      name: "Nhập tờ khai",
      code: "AAAA2222",
      id: 2
    },
    {
      name: "đóng phí làm hàng",
      code: "BBBB2222",
      id: 3
    },
    {
      name: "Nộp chứng từ 1",
      code: "CCCC1111",
      id: 4
    },
    {
      name: "Nhập tờ khai 2",
      code: "EEE2222",
      id: 5
    },
    {
      name: "đóng phí làm hàng 2",
      code: "BFAFBB2222",
      id: 6
    }
  ];

  selectedStages: any[] = [];

  tempStages1: any[] = [];
  tempStages2: any[] = [];

  constructor() {
    super();
  }


  onChangeCheckBoxStage($event: any, stage: any, index: number  ) {
    $event.target.checked  ? this.tempStages1.push(stage) : this.tempStages1.splice(index, 1);
  }

  onChangeCheckBoxStageSelected($event: any, stage: any, index: number) {
    $event.target.checked  ? this.tempStages2.push(stage) : this.tempStages2.splice(index, 1);
  }

  onAddStage() {
    this.selectedStages.push(...this.tempStages1);

    this.stages = this.stages.filter(
      (stage: any) => !_.includes(this.selectedStages, stage)
    );

    this.tempStages1 = [];
  }

  onRemoveStage() {
    this.stages.push(...this.tempStages2);

    this.selectedStages = this.selectedStages.filter(
      (stage: any) => !_.includes(this.stages, stage)
    );

    this.tempStages2 = [];
  }

  // dropped(event: CdkDragDrop<any[]>) {
  //     moveItemInArray(this.numbers, event.previousIndex, event.currentIndex);
  // }
}



class Stage {
  id: string;
  code: string;
  name: string;

  constructor(data?: any) {
    const self = this;
    _.each(data, (val, key) => {
        if (self.hasOwnProperty(key)) {
            self[key] = val;
        }
    });

  }
}

