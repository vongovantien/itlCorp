import { Component, OnInit, OnChanges, Input, EventEmitter, Output } from "@angular/core";
import { moveItemInArray, CdkDragDrop } from "@angular/cdk/drag-drop";

import { PopupBase } from "src/app/modal.base";
import { StageModel } from "src/app/shared/models/catalogue/stage.model";
import { JobRepo } from "src/app/shared/repositories";

import { takeUntil, catchError, finalize } from "rxjs/operators";
import { NgxSpinnerService } from "ngx-spinner";
import { ToastrService } from "ngx-toastr";
import * as _ from "lodash";
import { Stage } from "src/app/shared/models";

@Component({
  selector: "add-stage-popup",
  templateUrl: "./add-stage.popup.component.html",
  styleUrls: ["./add-stage.popup.component.scss"]
})
export class OpsModuleStageManagementAddStagePopupComponent extends PopupBase implements OnInit, OnChanges {

  @Input() stages: any[] = [];
  @Input() stageSelected: any[] = [];
  @Input() id: string = '';
  @Output() onSuccess: EventEmitter<any> = new EventEmitter<any>();

  selectedStages: any[] = [];
  tempStages1: any[] = [];
  tempStages2: any[] = [];

  keyword_stage: string = '';
  keyword_stageSelected: string = '';

  isAdd: boolean = false;
  constructor(
    private _jobRepo: JobRepo,
    private _spinner: NgxSpinnerService,
    private _toaster: ToastrService
  ) {
    super();
  }

  ngOnInit() {

  }

  ngOnChanges() {
    if (!!this.stageSelected.length) {
      this.selectedStages = this.stageSelected;
    }
  }


  onChangeCheckBoxStage($event: any, stage: StageModel, index: number) {
    $event.target.checked ? this.tempStages1.push(stage) : this.tempStages1.splice(index, 1);
  }

  onChangeCheckBoxStageSelected($event: any, stage: StageModel, index: number) {
    $event.target.checked ? this.tempStages2.push(stage) : this.tempStages2.splice(index, 1);
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

  onSaveStage() {
    const data: any[] = this.selectedStages.map((stage: any, index: number) => new Stage(stage));

    // assigne jobId
    for (const stage of data) {
      if (!stage.jobId) {
        stage.jobId = this.id;
      }
    }

    this._spinner.show();
    this._jobRepo.addStageToJob(this.id, data).pipe(
      takeUntil(this.ngUnsubscribe),
      catchError(this.catchError),
      finalize(() => { this._spinner.hide() }),
    ).subscribe(
      (res: any) => {
        if (!res.status) {
          this._toaster.success(res.message, '', { positionClass: 'toast-bottom-right' });
        } else {
          this.selectedStages = [];
          this._toaster.success(res.message, '', { positionClass: 'toast-bottom-right' });
          this.onSuccess.emit();
          this.hide();
        }
      },
      (errs: any) => {
      },
      () => { }
    )
  }

  onCancel() {
    this.hide();
    this.onSuccess.emit();
    this.stages.push(...this.selectedStages);

    this.selectedStages = this.selectedStages.filter(
      (stage: any) => !_.includes(this.stages, stage)
    );

    this.tempStages2 = [];
  }

  drop(event: CdkDragDrop<string[]>) {
    moveItemInArray(this.selectedStages, event.previousIndex, event.currentIndex);
  }

}

interface IStageUnique {
  code: string;
  id: string;
  jobId: string;
  orderNumberProcessed: number;
  stageId: number;
}



