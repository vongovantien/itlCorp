import { Component, ViewChild } from "@angular/core";
import { AppPage } from "src/app/app.base";
import { OpsModuleStageManagementAddStagePopupComponent } from "./add/add-stage.popup.component";
import { NgxSpinnerService } from "ngx-spinner";
import { OpsModuleStageManagementDetailComponent } from "./detail/detail-stage-popup.component";
import { OperationRepo, DocumentationRepo } from "src/app/shared/repositories";
import { ActivatedRoute } from "@angular/router";


import { catchError, finalize, takeUntil, tap } from 'rxjs/operators';
import { Stage } from "src/app/shared/models/operation/stage";
import { SortService } from "src/app/shared/services";
import { NgProgress } from "@ngx-progressbar/core";
import { AssignStagePopupComponent } from "./assign-stage/assign-stage.popup";

@Component({
    selector: "app-ops-module-stage-management",
    templateUrl: "./stage-management.component.html",
    styleUrls: ["./stage-management.component.scss"]
})
export class OpsModuleStageManagementComponent extends AppPage {

    data: any = null;
    @ViewChild(OpsModuleStageManagementAddStagePopupComponent, { static: false }) popupCreate: OpsModuleStageManagementAddStagePopupComponent;
    @ViewChild(OpsModuleStageManagementDetailComponent, { static: false }) popupDetail: OpsModuleStageManagementDetailComponent;
    @ViewChild(AssignStagePopupComponent, { static: false }) assignStagePopup: AssignStagePopupComponent;

    stages: Stage[] = [];
    stageAvailable: any[] = [];
    selectedStage: Stage = null;
    currentStages: Stage[] = [];

    jobId: string = '';

    timeOutSearch: any;
    constructor(
        private _spinner: NgxSpinnerService,
        private _operation: OperationRepo,
        private _activedRouter: ActivatedRoute,
        private _sortService: SortService,
        private _documentRepo: DocumentationRepo,
        private _ngProgressService: NgProgress,
    ) {
        super();
        this._progressRef = this._ngProgressService.ref();
    }

    ngOnInit() {
        this._activedRouter.params.subscribe((param: any) => {
            if (param.id) {
                this.jobId = param.id;
                this.getShipmentDetails(this.jobId);
                this.getListStageJob(this.jobId);
                this.getListStageAvailable(this.jobId);
            }
        });
    }

    getShipmentDetails(id: any) {
        this._progressRef.start();
        this._documentRepo.getDetailShipment(id)
            .pipe(
                catchError(this.catchError),
                finalize(() => this._progressRef.complete()),
            ).subscribe(
                (response: any) => {
                    this.data = response;
                },
            );
    }

    openPopUpCreateStage() {
        this.popupCreate.show();
    }

    openPopUpAssignStage() {
        this.assignStagePopup.show();
    }


    openPopupDetail() {
        this.popupDetail.show();
    }

    getListStageJob(id: string) {
        this._spinner.show();

        this._operation.getListStageOfJob(id).pipe(
            takeUntil(this.ngUnsubscribe),
            catchError(this.catchError),
            finalize(() => { this._spinner.hide(); }),
        ).subscribe(
            (res: any[]) => {
                if (res instanceof Error) {
                } else {
                    this.stages = this._sortService.sort(res.map((item: any) => new Stage(item)), 'orderNumberProcessed', true);
                    this.currentStages = this.stages;
                }
            },
        );
    }

    getListStageAvailable(id: string) {
        this._spinner.show();

        this._operation.getListStageNotAssigned(id).pipe(
            takeUntil(this.ngUnsubscribe),
            catchError(this.catchError),
            finalize(() => { this._spinner.hide(); }),
        ).subscribe(
            (res: any[]) => {
                if (res instanceof Error) {

                } else {
                    this.stageAvailable = res || [];
                }
            },
        );
    }

    getDetail(id: string) {
        this._operation.getDetailStageOfJob(id).pipe(
            takeUntil(this.ngUnsubscribe),
            catchError(this.catchError),
            finalize(() => { this._spinner.hide(); }),
        ).subscribe(
            (res: any[]) => {
                if (res instanceof Error) {

                } else {
                    this.selectedStage = new Stage(res);
                    this.openPopupDetail();
                }
            },
        );
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
