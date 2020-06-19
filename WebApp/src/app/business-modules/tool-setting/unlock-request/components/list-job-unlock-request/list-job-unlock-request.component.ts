import { Component, OnInit, Input, ViewChild } from "@angular/core";
import { AppList } from "src/app/app.list";
import { SortService } from "@services";
import { ToastrService } from "ngx-toastr";
import { UnlockRequestInputSearchJobPopupComponent } from "../popup/input-search-job/input-search-job.popup";
import { UnlockRequestInputSearchSettlementAdvancePopupComponent } from "../popup/input-search-settlement-advance/input-search-settlement-advance.popup";
import { CommonEnum } from "@enums";
import { SetUnlockRequestJobModel } from "@models";

@Component({
    selector: 'list-job-unlock-request',
    templateUrl: './list-job-unlock-request.component.html',
})
export class UnlockRequestListJobComponent extends AppList implements OnInit {
    @ViewChild(UnlockRequestInputSearchJobPopupComponent, { static: false }) inputSearchJobPopup: UnlockRequestInputSearchJobPopupComponent;
    @ViewChild(UnlockRequestInputSearchSettlementAdvancePopupComponent, { static: false }) inputSearchSettlementAdvancePopup: UnlockRequestInputSearchSettlementAdvancePopupComponent;
    @Input() unlockType: CommonEnum.UnlockTypeEnum;

    dataJobs: SetUnlockRequestJobModel[] = [];

    constructor(
        private _sortService: SortService,
        private _toastService: ToastrService
    ) {
        super();

    }

    ngOnInit(): void {
        this.headers = [
            { title: 'Unlock Name', field: 'unlockName', sortable: true },
            { title: 'Reason', field: 'reason', sortable: true }
        ];
    }

    addDetail() {
        console.log(this.unlockType);
        if (this.unlockType === CommonEnum.UnlockTypeEnum.ADVANCE || this.unlockType === CommonEnum.UnlockTypeEnum.SETTEMENT) {
            this.inputSearchSettlementAdvancePopup.unlockType = this.unlockType;
            this.inputSearchSettlementAdvancePopup.show();
        }
        if (this.unlockType === CommonEnum.UnlockTypeEnum.SHIPMENT || this.unlockType === CommonEnum.UnlockTypeEnum.CHANGESERVICEDATE) {
            this.inputSearchJobPopup.show();
        }
    }

    onAddDetail(jobs: SetUnlockRequestJobModel[]) {
        console.log(jobs);
        if (!this.detectDuplicateCharge([...this.dataJobs, ...jobs])) {
            this.dataJobs = [...this.dataJobs, ...jobs];
        } else {
            this._toastService.warning("Job/Advance/Settlement has existed in list");
            return;
        }
    }

    detectDuplicateCharge(jobs: SetUnlockRequestJobModel[]) {
        if (!jobs.length) {
            return false;
        }
        return this.utility.checkDuplicateInObject('unlockName', jobs);
    }

    checkUncheckAllCharge() {
        for (const job of this.dataJobs) {
            job.isSelected = this.isCheckAll;
        }
    }

    onChangeCheckBoxCharge() {
        this.isCheckAll = this.dataJobs.every((item: SetUnlockRequestJobModel) => item.isSelected);
    }

    removeJob() {
        this.dataJobs = this.dataJobs.filter((item: SetUnlockRequestJobModel) => !item.isSelected);
        this.isCheckAll = false;
    }
}