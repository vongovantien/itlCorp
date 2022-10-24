import { Component, EventEmitter, Output } from '@angular/core';
import { FormGroup } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { catchError, map } from 'rxjs/operators';
import { PopupBase } from 'src/app/popup.base';
import { User } from 'src/app/shared/models';
import { CatalogueRepo, DocumentationRepo, OperationRepo, SystemRepo } from 'src/app/shared/repositories';
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

    configHbl: CommonInterface.IComboGirdConfig = {
        placeholder: 'Please select',
        displayFields: [
            { field: 'hwbno', label: 'HBL No' },
            { field: 'customerName', label: 'Customer Name' },
        ],
        dataSource: [],
        selectedDisplayFields: ['hwbno'],
    };
    selectedHbl: Partial<CommonInterface.IComboGridData> = {};
    selectedHblData: any;

    users: User[] = [];
    description: string = '';
    isSubmitted: boolean = false;
    jobId: string = '';
    isAsignment: boolean = false;

    constructor(
        private _catalogueRepo: CatalogueRepo,
        private _dataService: DataService,
        private _sysRepo: SystemRepo,
        private _operationRepo: OperationRepo,
        private _toastService: ToastrService,
        private _document: DocumentationRepo,
        private route: ActivatedRoute
    ) {
        super();
    }

    ngOnInit(): void {
        this.jobId = this.route.snapshot.paramMap.get('jobId');
        this.getStage();
        this.getListUser();
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

    getHblList(jobId: string) {
        this._document.getListHouseBillOfJob({ jobId: jobId })
            .pipe(catchError(this.catchError))
            .subscribe(
                (res: any) => {
                    this.configHbl.dataSource = res;
                    console.log(res)
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

    assignStage() {
        this.isSubmitted = true;
        if (!this.selectedUser.value || !this.selectedStage.value || !this.selectedHbl.value) {
            return;
        }
        const body: IAssignStage = {
            id: "00000000-0000-0000-0000-000000000000",
            jobId: this.jobId,
            hblId: this.selectedHblData.id,
            stageId: this.selectedStageData.id,
            mainPersonInCharge: this.selectedUserData.id,
            description: this.description,
            type: 'User'
        };
        this._operationRepo.assignStageOPS(body).pipe(catchError(this.catchError))
            .subscribe(
                (res: CommonInterface.IResult) => {
                    if (res.status) {
                        this._toastService.success(res.message);
                        this.onAssign.emit(this.selectedStageData.jobId);
                        this.closePopup();
                    } else {
                        this._toastService.warning(res.message);
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
            case "hbl":
                this.selectedHbl = {};
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
            case "hbl":
                this.selectedHblData = data;
                this.selectedHbl = { field: 'hwbno', value: data.hwbno };
            default:
                break;
        }
    }

    closePopup() {
        this.hide();

        // * Reset value
        this.description = '';
        // this.selectedUser = null;
        this.selectedStage = {};
        this.selectedUser = {};
        this.selectedHbl = {};
        this.isSubmitted = false;
    }
}

interface IAssignStage {
    id: string;
    jobId: string;
    hblId: string;
    stageId: number;
    mainPersonInCharge: string;
    description: string;
    type: string;
}
