import { Component, EventEmitter, Output } from '@angular/core';
import { FormGroup } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { CsTransactionDetail, Stage } from '@models';
import { ToastrService } from 'ngx-toastr';
import { catchError, map } from 'rxjs/operators';
import { PopupBase } from 'src/app/popup.base';
import { User } from 'src/app/shared/models';
import { CatalogueRepo, DocumentationRepo, SystemRepo } from 'src/app/shared/repositories';
import { DataService } from 'src/app/shared/services';
import { SystemConstants } from 'src/constants/system.const';

@Component({
    selector: 'share-asign-stage-popup',
    templateUrl: './assign-stage.popup.html',
})
export class ShareBusinessAssignStagePopupComponent extends PopupBase {

    @Output() onAssign: EventEmitter<any> = new EventEmitter<any>();

    formAssignStage: FormGroup;

    configStage: CommonInterface.IComboGirdConfig = {
        placeholder: 'Please select',
        displayFields: [
            { field: 'code', label: 'Stage Code' },
            { field: 'stageNameEn', label: 'Stage Name' },
        ],
        dataSource: [],
        selectedDisplayFields: ['stageNameEn'],
    };
    selectedStage: Partial<CommonInterface.IComboGridData> = {};
    selectedStageData: any;

    configUser: CommonInterface.IComboGirdConfig = {
        placeholder: 'Please select',
        displayFields: [
            { field: 'username', label: 'User Name' },
            { field: 'employeeNameVn', label: 'Full Name' },
        ],
        dataSource: [],
        selectedDisplayFields: ['username'],
    };
    selectedUser: Partial<CommonInterface.IComboGridData> = {};
    selectedUserData: any;

    users: User[] = [];
    description: string = '';
    isSubmitted: boolean = false;
    jobId: string = '';
    isAssignment: boolean = false;
    houseBillList: CsTransactionDetail[];
    selectedHbl: any[];

    constructor(
        private _catalogueRepo: CatalogueRepo,
        private _dataService: DataService,
        private _sysRepo: SystemRepo,
        private _toastService: ToastrService,
        private _documentRepo: DocumentationRepo,
        private _activedRouter: ActivatedRoute
    ) {
        super();
    }

    ngOnInit(): void {
        this.getStage();
        this.getListUser();
        this._activedRouter.params.subscribe((res: any) => {
            if (!!res.jobId) {
                this.getHblList(res.jobId)
                this.jobId = res.jobId;
            }
        });
    }

    getHblList(jobId: string) {
        this._documentRepo.getHBLOfJob({ jobId: jobId })
            .pipe(catchError(this.catchError))
            .subscribe(
                (res: any) => {
                    if (!!res && res.length > 0) {
                        this.houseBillList = [new CsTransactionDetail({ id: 'All', hwbno: 'All' })]
                        this.houseBillList = this.houseBillList.concat(res);
                        this.houseBillList = this.houseBillList.filter(x => x.hwbno !== 'N/H').concat(this.houseBillList.find(x => x.hwbno === 'N/H') || []);

                        console.log(this.houseBillList)
                    }
                }
            );
    }

    getStage() {
        this._catalogueRepo.getStage({ active: true })
            .pipe(catchError(this.catchError))
            .subscribe(
                (res: any) => {
                    this.configStage.dataSource = res;
                },
                () => { }
            );
    }

    getListUser() {
        if (!!this._dataService.getDataByKey(SystemConstants.CSTORAGE.SYSTEM_USER)) {
            this.users = this._dataService.getDataByKey(SystemConstants.CSTORAGE.SYSTEM_USER) || [];
            // this.users = <any>this.utility.prepareNg2SelectData(this.users, 'id', 'username');

        } else {
            this._sysRepo.getListSystemUser()
                .pipe(
                    catchError(this.catchError),
                    map((data: any[]) => data.map((item: any) => new User(item))),
                )
                .subscribe(
                    (data: any) => {
                        // this.users = data || [];

                        // this.users = <any>this.utility.prepareNg2SelectData(this.users, 'id', 'username');
                        this.configUser.dataSource = data || [];
                    },
                );
        }
    }

    assignMultipleStage() {
        this.isSubmitted = true;
        if (!this.selectedUser.value || !this.selectedStage.value || ((!this.selectedHbl || this.selectedHbl?.length <= 0) && this.selectedStage.value !== 'Make Advance/ Settlement')) {
            return;
        }

        let listItemTemp = [];
        if (!!this.selectedHbl && this.selectedHbl[0] === 'All') {
            listItemTemp = this.houseBillList.filter(x => x.id !== 'All').map(x => x.id)
        }
        else if (!!this.selectedHbl && this.selectedHbl[0] !== 'All') {
            listItemTemp = this.selectedHbl
        } else {
            listItemTemp.push(new Stage().id)
        }

        const body: any[] = listItemTemp.map((stage: any, index: number) => new Stage(stage));

        for (const [index, value] of <any>body.entries()) {
            value.id = "00000000-0000-0000-0000-000000000000";
            value.jobId = this.jobId;
            value.hblId = listItemTemp[index];
            value.stageId = this.selectedStageData.id;
            value.mainPersonInCharge = this.selectedUserData.id;
        }

        this._documentRepo.addMultipleStageToJob(this.jobId, body).pipe(catchError(this.catchError))
            .subscribe(
                (res: CommonInterface.IResult) => {
                    if (res.status) {
                        this._toastService.success(res.message);
                        this.onAssign.emit(this.selectedStageData.jobId);
                        this.closePopup();
                    } else {
                        this._toastService.error(res.message);
                    }
                },
            );
    }

    onRemoveData(type: string) {
        switch (type) {
            case "stage":
                this.selectedStage = {};
                break;
            case "user":
                this.selectedUser = {};
                break;
            default:
                break;
        }
    }

    onSelectData(data: any, type: string) {
        switch (type) {
            case "stage":
                this.selectedStageData = data;
                this.selectedStage = { field: 'stageNameEn', value: data.stageNameEn };
                break;
            case "user":
                this.selectedUserData = data;
                this.selectedUser = { field: 'username', value: data.username };
                break;
        }
    }

    closePopup() {
        this.hide();
        // * Reset value
        this.description = '';
        this.selectedStage = {};
        this.selectedUser = {};
        this.selectedHbl = null;
        this.isSubmitted = false;
    }

    onSelectDataFormInfo($event: any) {
        if ($event.length > 0) {
            if ($event[$event.length - 1].id === 'All') {
                this.selectedHbl = []
                this.selectedHbl.push(this.houseBillList[0].id);
            }
            else {
                this.selectedHbl = this.selectedHbl.filter(x => x !== 'All');
            }
        }
    }
}
