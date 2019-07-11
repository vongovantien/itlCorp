import { Component, OnInit, ViewChild } from "@angular/core";
import { AppPage } from "src/app/app.base";
import { OpsModuleStageManagementAddStagePopupComponent } from "./add/add-stage.popup.component";
import { NgxSpinnerService } from "ngx-spinner";
import { OpsModuleStageManagementDetailComponent } from "./detail/detail-stage-popup.component";
import { JobRepo } from "src/app/shared/repositories";
import { ActivatedRoute } from "@angular/router";


import { catchError, finalize, takeUntil } from 'rxjs/operators';
import { Stage } from "src/app/shared/models/operation/stage";

@Component({
    selector: "app-ops-module-stage-management",
    templateUrl: "./stage-management.component.html",
    styleUrls: ["./stage-management.component.scss"]
})
export class OpsModuleStageManagementComponent extends AppPage {

    @ViewChild(OpsModuleStageManagementAddStagePopupComponent, { static: false }) popupCreate: OpsModuleStageManagementAddStagePopupComponent;
    @ViewChild(OpsModuleStageManagementDetailComponent, {static: false}) popupDetail: OpsModuleStageManagementDetailComponent;

    stages: Stage[] = [];
    stageAvailable: any[] = [];
    selectedStage: Stage = null;

    jobId: string = '';
    
    constructor(
        private _spinner: NgxSpinnerService,
        private _jobRepo: JobRepo,
        private _activedRouter: ActivatedRoute
    ) {
        super();
    }

    ngOnInit() {
        this._activedRouter.params.subscribe( (param: any) => {
            if(param.id) {
                this.jobId = param.id;
                this.getListStageJob(this.jobId);
                this.getListStageAvailable(this.jobId);
            }
        })
    }

    openPopUpCreateStage() {
        this.popupCreate.show();
    }

    openPopupDetail() {
        this.popupDetail.show()
    }

    getListStageJob(id: string) {
        this._spinner.show();

       this._jobRepo.getListStageOfJob(id).pipe(
            takeUntil(this.ngUnsubscribe),
            catchError(this.catchError),
            finalize(() => { this._spinner.hide() }),
        ).subscribe(
            (res: any[]) => {
                if (res instanceof Error) {
                    
                }else {
                    this.stages = res.map( (item: any) => new Stage(item));
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

    getListStageAvailable(id: string) {
        this._spinner.show();

        this._jobRepo.getListStageNotAssigned(id).pipe(
             takeUntil(this.ngUnsubscribe),
             catchError(this.catchError),
             finalize(() => { this._spinner.hide() }),
         ).subscribe(
             (res: any[]) => {
                 if (res instanceof Error) {
                     
                 } else {
                     this.stageAvailable = res || [];
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

    getDetail(id: string) {
        this._jobRepo.getDetailStageOfJob(id).pipe(
            takeUntil(this.ngUnsubscribe),
            catchError(this.catchError),
            finalize(() => { this._spinner.hide() }),
        ).subscribe(
            (res: any[]) => {
                if (res instanceof Error) {
                    
                } else {
                    this.selectedStage = new Stage(res);
                    this.popupDetail.show();
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

    //when add success stage into job
    onAddSuccess() {
        this.getListStageJob(this.jobId);
        this.getListStageAvailable(this.jobId);
    }
}
