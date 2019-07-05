import { Component, OnInit, OnChanges, Input, EventEmitter, Output } from "@angular/core";
import { moveItemInArray, CdkDragDrop } from "@angular/cdk/drag-drop";

import { PopupBase } from "src/app/modal.base";
import { StageModel } from "src/app/shared/models/catalogue/stage.model";
import { JobRepo } from "src/app/shared/repositories";

import { takeUntil, catchError, finalize } from "rxjs/operators";
import { NgxSpinnerService } from "ngx-spinner";
import { ToastrService } from "ngx-toastr";
import * as _ from "lodash";

@Component({
  selector: "add-stage-popup",
  templateUrl: "./add-stage.popup.component.html",
  styleUrls: ["./add-stage.popup.component.scss"]
})
export class OpsModuleStageManagementAddStagePopupComponent extends PopupBase implements OnInit, OnChanges {

  @Input() stages: any[] = [];
  @Input() id: string = '';
  @Output() onSuccess: EventEmitter<any> = new EventEmitter<any>();

  selectedStages: any[] = [];
  tempStages1: any[] = [];
  tempStages2: any[] = [];

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
    if(!!this.stages.length) {
    }
  }


  onChangeCheckBoxStage($event: any, stage: StageModel, index: number  ) {
    $event.target.checked  ? this.tempStages1.push(stage) : this.tempStages1.splice(index, 1);
  }

  onChangeCheckBoxStageSelected($event: any, stage: StageModel, index: number) {
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

  onSaveStage() {
    this.selectedStages = this.selectedStages.map( (stage: StageModel,index: number) => {
      return ({
        id: '00000000-0000-0000-0000-000000000000', //TODO
        orderNumberProcessed: index,
        code: stage.code,
        stageId: stage.id,
        jobId: this.id
      })
    });

    this._spinner.show();
    this._jobRepo.addStageToJob(this.id, this.selectedStages).pipe(
      takeUntil(this.ngUnsubscribe),
      catchError(this.catchError),
      finalize(() => { this._spinner.hide() }),
    ).subscribe(
        (res: any) => {
          if (!res.status) {
            this._toaster.success(res.message, '', { positionClass: 'toast-bottom-right' });
          }else {
            // remove stages selected
            this.selectedStages = [];
            this._toaster.success(res.message, '', { positionClass: 'toast-bottom-right' });
            this.onSuccess.emit();
            this.hide();
          }
      },
      // error
      (errs: any) => {
          // this.handleErrors(errs)
      },
      // complete
      () => { }
    )
  }

  onCancel() {
    this.hide();
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



