import { Component, ViewChild } from "@angular/core";
import { ToastrService } from "ngx-toastr";
import { Router } from "@angular/router";
import { NgProgress } from "@ngx-progressbar/core";
import { FormBuilder, FormGroup, AbstractControl, Validators } from "@angular/forms";
import { AppForm } from "src/app/app.form";
import { SettingRepo } from "@repositories";
import { UnlockRequestListJobComponent } from "../components/list-job-unlock-request/list-job-unlock-request.component";
import { User, SetUnlockRequestModel } from "@models";
import { SystemConstants } from "@constants";
import { CommonEnum } from "@enums";
import { SelectItem } from "ng2-select";
import { catchError } from "rxjs/operators";
import { formatDate } from "@angular/common";

@Component({
    selector: 'app-unlock-request-add',
    templateUrl: './add-unlock-request.component.html'
})

export class UnlockRequestAddNewComponent extends AppForm {
    @ViewChild(UnlockRequestListJobComponent, { static: false }) listJobComponent: UnlockRequestListJobComponent;
    formAdd: FormGroup;
    subject: AbstractControl;
    requester: AbstractControl;
    unlockType: AbstractControl;
    serviceDate: AbstractControl;
    generalReason: AbstractControl;

    unlockTypeEnum: CommonEnum.UnlockTypeEnum = CommonEnum.UnlockTypeEnum.SHIPMENT;

    unlockTypeList: CommonInterface.INg2Select[] = [
        { id: 'Shipment', text: 'Shipment' },
        { id: 'Advance', text: 'Advance' },
        { id: 'Settlement', text: 'Settlement' },
        { id: 'Change Service Date', text: 'Change Service Date' }
    ];
    isSubmited: boolean = false;
    userLogged: User;
    tableDefault: string = `<table style="width: 100%;"><tbody><tr><td style="width: 50%; text-align: left;">JOB ID
				<br><br></td><td style="width: 50.0000%;"><br></td></tr><tr><td style="width: 50.0000%;">CHARGE NAME
                <br><br></td><td style="width: 50.0000%;"><br></td></tr><tr><td style="width: 50.0000%;"><span style="color: red;">CURRENT VALUE</span><br><br></td><td style="width: 50.0000%;">
                <br></td></tr><tr><td style="width: 50.0000%;"><span style="color: red;">UPDATE VALUE</span><br><br></td><td style="width: 50.0000%;"><br></td></tr><tr><td style="width: 50.0000%;"><span style="color: red;">PROFIT BEFORE REVISING</span>
                <br><br></td><td style="width: 50.0000%;">
                <br></td></tr><tr><td style="width: 50.0000%;"><span style="color: red;">PROFIT AFTER REVISING</span><br><br></td><td style="width: 50.0000%;"><br></td></tr><tr><td style="width: 50.0000%;">REASON
				<br><br></td><td style="width: 50.0000%;"><br></td></tr><tr><td style="width: 50.0000%;">ANY OTHER SPECIAL REQUIREMENT
				<br><br></td><td style="width: 50.0000%;"><br></td></tr></tbody></table>`;
    constructor(
        private _toastService: ToastrService,
        private _router: Router,
        private _progressService: NgProgress,
        private _fb: FormBuilder,
        private _settingRepo: SettingRepo,
    ) {
        super();
        this._progressRef = this._progressService.ref();
    }

    ngOnInit() {
        this.initForm();
        this.getUserLogged();
    }

    initForm() {
        this.formAdd = this._fb.group({
            subject: ["Unlock Shipment", Validators.required],
            requester: [null, Validators.required],
            unlockType: [[this.unlockTypeList[0]]],
            serviceDate: [{
                startDate: new Date(),
                endDate: new Date(),
            }],
            generalReason: [this.tableDefault]
        });

        this.subject = this.formAdd.controls['subject'];
        this.requester = this.formAdd.controls['requester'];
        this.unlockType = this.formAdd.controls['unlockType'];
        this.serviceDate = this.formAdd.controls['serviceDate'];
        this.generalReason = this.formAdd.controls['generalReason'];
    }

    getUserLogged() {
        this.userLogged = JSON.parse(localStorage.getItem(SystemConstants.USER_CLAIMS));
        this.requester.setValue(this.userLogged.preferred_username);
    }

    selectedUnlockType(e: SelectItem) {
        switch (e.id) {
            case "Shipment":
                this.unlockTypeEnum = CommonEnum.UnlockTypeEnum.SHIPMENT;
                this.generalReason.setValue(this.tableDefault);
                break;
            case "Advance":
                this.unlockTypeEnum = CommonEnum.UnlockTypeEnum.ADVANCE;
                break;
            case "Settlement":
                this.unlockTypeEnum = CommonEnum.UnlockTypeEnum.SETTEMENT;
                break;
            case "Change Service Date":
                this.unlockTypeEnum = CommonEnum.UnlockTypeEnum.CHANGESERVICEDATE;
                break;
            default:
                break;
        }
    }

    save() {
        this.isSubmited = true;
        console.log(this.listJobComponent.dataJobs);
        if (this.formAdd.valid) {
            if (!this.listJobComponent.dataJobs.length) {
                this._toastService.warning("Unlock request don't have any job/advance/settlement in this period, Please check it again!");
                return;
            }
            const _unlockRequest: SetUnlockRequestModel = {
                id: SystemConstants.EMPTY_GUID,
                subject: this.subject.value,
                requester: null,
                unlockType: this.unlockType.value[0].id,
                newServiceDate: this.serviceDate.value && this.unlockType.value[0].id === 'Change Service Date' ? (this.serviceDate.value.startDate !== null ? formatDate(this.serviceDate.value.startDate, 'yyyy-MM-dd', 'en') : null) : null,
                generalReason: this.generalReason.value,
                requestDate: null,
                requestUser: null,
                statusApproval: null,
                userCreated: null,
                datetimeCreated: null,
                userModified: null,
                datetimeModified: null,
                groupId: 0,
                departmentId: 0,
                officeId: SystemConstants.EMPTY_GUID,
                companyId: SystemConstants.EMPTY_GUID,
                jobs: this.listJobComponent.dataJobs
            };
            console.log(_unlockRequest);
            this._settingRepo.addNewUnlockRequest(_unlockRequest)
                .pipe(catchError(this.catchError))
                .subscribe(
                    (res: CommonInterface.IResult) => {
                        if (res.status) {
                            this._toastService.success(res.message);
                        } else {
                            this._toastService.error(res.message);
                        }
                    }
                );

        }
    }

    sendRequest() {

    }
}