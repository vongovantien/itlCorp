import { Component, OnInit, Output, EventEmitter } from "@angular/core";
import { AppForm } from "src/app/app.form";
import { FormBuilder, FormGroup, AbstractControl } from "@angular/forms";
import { SystemRepo } from "@repositories";
import { catchError } from "rxjs/operators";
import { formatDate } from "@angular/common";
import { UnlockRequestCriteria, User } from "@models";
import { CommonEnum } from "@enums";
import { SystemConstants } from "@constants";

@Component({
    selector: 'form-search-unlock-request',
    templateUrl: './form-search-unlock-request.component.html'
})

export class UnlockRequestFormSearchComponent extends AppForm implements OnInit {
    @Output() onSearch: EventEmitter<UnlockRequestCriteria> = new EventEmitter<UnlockRequestCriteria>();

    formSearch: FormGroup;
    referenceNo: AbstractControl;
    unlockType: AbstractControl;
    requester: AbstractControl;
    createdDate: AbstractControl;
    unlockStatus: AbstractControl;

    requesterList: any[] = [];
    displayFieldsRequester: CommonInterface.IComboGridDisplayField[] = [
        { field: 'username', label: 'User Name' },
        { field: 'employeeNameVn', label: 'Full Name' }
    ];

    unlockTypeList: CommonInterface.INg2Select[] = [
        { id: 'Shipment', text: 'Shipment' },
        { id: 'Advance', text: 'Advance' },
        { id: 'Settlement', text: 'Settlement' },
        { id: 'Change Service Date', text: 'Change Service Date' }
    ];

    unlockStatusList: CommonInterface.INg2Select[] = [
        { id: 'New', text: 'New' },
        { id: 'Request Approval', text: 'Request Approval' },
        { id: 'Leader Approved', text: 'Leader Approved' },
        { id: 'Manager Approved', text: 'Manager Approved' },
        { id: 'Accountant Approved', text: 'Accountant Approved' },
        { id: 'Denied', text: 'Denied' },
        { id: 'Done', text: 'Done' },
    ];
    userLogged: User;
    constructor(
        private _systemRepo: SystemRepo,
        private _fb: FormBuilder,
    ) {
        super();
    }

    ngOnInit() {
        this.initFormSearch();
        this.getUserLogged();
        this.getUsers();
    }

    initFormSearch() {
        this.formSearch = this._fb.group({
            referenceNo: [],
            unlockType: [],
            requester: [],
            createdDate: [],
            unlockStatus: []
        });
        this.referenceNo = this.formSearch.controls['referenceNo'];
        this.unlockType = this.formSearch.controls['unlockType'];
        this.requester = this.formSearch.controls['requester'];
        this.createdDate = this.formSearch.controls['createdDate'];
        this.unlockStatus = this.formSearch.controls['unlockStatus'];
    }

    getUsers() {
        // this._systemRepo.getSystemUsers({ active: true })
        this._systemRepo.getSystemUsers()
            .pipe(catchError(this.catchError))
            .subscribe(
                (data: any) => {
                    this.requesterList = data.map(x => ({ "text": x.username, "id": x.id }));
                },
            );
    }

    getUserLogged() {
        this.userLogged = JSON.parse(localStorage.getItem(SystemConstants.USER_CLAIMS));
    }

    onSubmit() {
        let _unlockTypeEnum = 0;
        const unlockType = this.unlockType.value ? (this.unlockType.value.length > 0 ? this.unlockType.value[0].id : null) : null
        switch (unlockType) {
            case "Shipment":
                _unlockTypeEnum = CommonEnum.UnlockTypeEnum.SHIPMENT;
                break;
            case "Advance":
                _unlockTypeEnum = CommonEnum.UnlockTypeEnum.ADVANCE;
                break;
            case "Settlement":
                _unlockTypeEnum = CommonEnum.UnlockTypeEnum.SETTEMENT;
                break;
            case "Change Service Date":
                _unlockTypeEnum = CommonEnum.UnlockTypeEnum.CHANGESERVICEDATE;
                break;
            default:
                break;
        }
        const body: UnlockRequestCriteria = {
            referenceNos: !!this.referenceNo.value ? this.referenceNo.value.trim().replace(/(?:\r\n|\r|\n|\\n|\\r)/g, ',').trim().split(',').map((item: any) => item.trim()) : null,
            unlockTypeNum: _unlockTypeEnum,
            requester: this.requester.value ? (this.requester.value.length > 0 ? this.requester.value[0].id : this.userLogged.id) : this.userLogged.id,
            createdDate: this.createdDate.value ? (this.createdDate.value.startDate !== null ? formatDate(this.createdDate.value.startDate, 'yyyy-MM-dd', 'en') : null) : null,
            statusApproval: this.unlockStatus.value ? (this.unlockStatus.value.length > 0 ? this.unlockStatus.value[0].id : null) : null,
        };
        this.onSearch.emit(body);
    }

    search() {
        this.onSubmit();
    }

    reset() {
        this.formSearch.reset();
        // tslint:disable-next-line: no-any
        this.onSearch.emit(<any>{ requester: this.userLogged.id });
    }
}
