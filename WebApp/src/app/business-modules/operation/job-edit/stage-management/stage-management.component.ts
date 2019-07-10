import { Component, OnInit, ViewChild, Input } from "@angular/core";
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

    @Input() data: any = null;
    @ViewChild(OpsModuleStageManagementAddStagePopupComponent, { static: false }) popupCreate: OpsModuleStageManagementAddStagePopupComponent;
    @ViewChild(OpsModuleStageManagementDetailComponent, { static: false }) popupDetail: OpsModuleStageManagementDetailComponent;

    stages: Stage[] = [];
    stageAvailable: any[] = [];
    selectedStage: Stage = null;
    currentStages: Stage[] = [];

    jobId: string = '';

    timeOutSearch: any;
    constructor(
        private _spinner: NgxSpinnerService,
        private _jobRepo: JobRepo,
        private _activedRouter: ActivatedRoute
    ) {
        super();
    }

    ngOnInit() {
        this._activedRouter.params.subscribe((param: any) => {
            if (param.id) {
                this.jobId = param.id;
                this.getListStageJob(this.jobId);
                this.getListStageAvailable(this.jobId);
            }
        });
    }

    ngOnChanges() {

    }

    openPopUpCreateStage() {
        this.popupCreate.show({ backdrop: 'static', keyboard: true });
    }

    openPopupDetail() {
        this.popupDetail.show({ backdrop: 'static', keyboard: true });
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
                } else {
                    this.stages = res.map((item: any) => new Stage(item));
                    this.currentStages = this.stages;
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
                    this.openPopupDetail();
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

    onSearching(keyword: string) {
        clearTimeout(this.timeOutSearch);
        this.timeOutSearch = setTimeout(() => {
            this.stages = this.currentStages;
            if (keyword.length >= 3) {
                this.stages = this.stages.filter((item: Stage) => {
                    if (!!item) {
                        return item.stageCode.toLowerCase().search(keyword.toLowerCase().trim()) !== -1
                            || (item.stageNameEN || '').toLowerCase().search(keyword.toLowerCase().trim()) !== -1
                            || (item.description || '').toLowerCase().search(keyword.toLowerCase().trim()) !== -1;
                    }
                });
            } else {
                this.stages = this.currentStages;
            }
        }, 500);


    }

    // when add success stage into job
    onAddSuccess() {
        this.getListStageJob(this.jobId);
        this.getListStageAvailable(this.jobId);
    }
}
