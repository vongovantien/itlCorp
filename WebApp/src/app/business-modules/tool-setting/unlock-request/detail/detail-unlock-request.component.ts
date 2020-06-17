import { Component, ViewChild } from "@angular/core";
import { ToastrService } from "ngx-toastr";
import { Router } from "@angular/router";
import { FormBuilder, FormGroup, AbstractControl } from "@angular/forms";
import { NgProgress } from "@ngx-progressbar/core";
import { AppForm } from "src/app/app.form";
import { SettingRepo } from "@repositories";
import { UnlockRequestListJobComponent } from "../components/list-job-unlock-request/list-job-unlock-request.component";
import { CommonEnum } from "@enums";
import { UnlockRequestInputDeniedCommentPopupComponent } from "../components/popup/input-denied-comment/input-denied-comment.popup";

@Component({
    selector: 'app-unlock-request-detail',
    templateUrl: './detail-unlock-request.component.html'
})

export class UnlockRequestDetailComponent extends AppForm {
    @ViewChild(UnlockRequestListJobComponent, { static: false }) listJobComponent: UnlockRequestListJobComponent;
    @ViewChild(UnlockRequestInputDeniedCommentPopupComponent, { static: false }) inputDeniedCommentPopup: UnlockRequestInputDeniedCommentPopupComponent;
    formDetail: FormGroup;
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

    constructor(
        private _toastService: ToastrService,
        private _router: Router,
        private _fb: FormBuilder,
        private _progressService: NgProgress,
        private _settingRepo: SettingRepo,
    ) {
        super();
        this._progressRef = this._progressService.ref();
    }
    ngOnInit() {
        this.unlockTypeActive = [this.unlockTypeList[0]];
        this.initForm();
    }

    initForm() {
        this.formDetail = this._fb.group({
            subject: [],
            requester: [],
            unlockType: [this.unlockTypeActive],
            serviceDate: [{
                startDate: this.createMoment().toDate(),
                endDate: this.createMoment().toDate(),
            }],
            generalReason: []
        });

        this.subject = this.formDetail.controls['subject'];
        this.requester = this.formDetail.controls['requester'];
        this.unlockType = this.formDetail.controls['unlockType'];
        this.serviceDate = this.formDetail.controls['serviceDate'];
        this.generalReason = this.formDetail.controls['generalReason'];
    }


    selectedUnlockType() {
    }

    save() {

    }

    sendRequest() {

    }

    cancelRequest() {

    }

    confirmRequest() {

    }

    denyRequest() {
        this.inputDeniedCommentPopup.show();
    }
}