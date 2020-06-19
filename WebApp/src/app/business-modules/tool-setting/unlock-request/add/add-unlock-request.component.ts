import { Component, ViewChild } from "@angular/core";
import { ToastrService } from "ngx-toastr";
import { Router } from "@angular/router";
import { NgProgress } from "@ngx-progressbar/core";
import { FormBuilder, FormGroup, AbstractControl } from "@angular/forms";
import { AppForm } from "src/app/app.form";
import { SettingRepo } from "@repositories";
import { UnlockRequestListJobComponent } from "../components/list-job-unlock-request/list-job-unlock-request.component";
import { User } from "@models";
import { SystemConstants } from "@constants";
import { CommonEnum } from "@enums";

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

    unlockTypeList: CommonInterface.INg2Select[] = [
        { id: 'Shipment', text: 'Shipment' },
        { id: 'Advance', text: 'Advance' },
        { id: 'Settlement', text: 'Settlement' },
        { id: 'Change Service Date', text: 'Change Service Date' }
    ];
    unlockTypeActive: any[] = [];
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
        this.unlockTypeActive = [this.unlockTypeList[0]];
        this.initForm();
        this.getUserLogged();
    }

    initForm() {
        this.formAdd = this._fb.group({
            subject: [],
            requester: [],
            unlockType: [this.unlockTypeActive],
            serviceDate: [{
                startDate: this.createMoment().toDate(),
                endDate: this.createMoment().toDate(),
            }],
            generalReason: []
        });

        this.subject = this.formAdd.controls['subject'];
        this.requester = this.formAdd.controls['requester'];
        this.unlockType = this.formAdd.controls['unlockType'];
        this.serviceDate = this.formAdd.controls['serviceDate'];
        this.generalReason = this.formAdd.controls['generalReason'];
        if (this.unlockTypeActive[0].id === CommonEnum.unlockTypeEnum.SHIPMENT) {
            this.generalReason.setValue(this.tableDefault);
        }
    }

    getUserLogged() {
        this.userLogged = JSON.parse(localStorage.getItem(SystemConstants.USER_CLAIMS));
        this.requester.setValue(this.userLogged.preferred_username);
    }

    selectedUnlockType() {
        if (this.unlockType.value[0].id === CommonEnum.unlockTypeEnum.SHIPMENT) {
            this.generalReason.setValue(this.tableDefault);
        }
    }

    save() {

    }

    sendRequest() {

    }
}