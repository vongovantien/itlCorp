import { Component, OnInit, Input, ViewChild } from "@angular/core";
import { AppList } from "src/app/app.list";
import { SortService } from "@services";
import { ToastrService } from "ngx-toastr";
import { UnlockRequestInputSearchJobPopupComponent } from "../popup/input-search-job/input-search-job.popup";
import { UnlockRequestInputSearchSettlementAdvancePopupComponent } from "../popup/input-search-settlement-advance/input-search-settlement-advance.popup";
import { CommonEnum } from "@enums";

@Component({
    selector: 'list-job-unlock-request',
    templateUrl: './list-job-unlock-request.component.html',
})
export class UnlockRequestListJobComponent extends AppList implements OnInit {
    @ViewChild(UnlockRequestInputSearchJobPopupComponent, { static: false }) inputSearchJobPopup: UnlockRequestInputSearchJobPopupComponent;
    @ViewChild(UnlockRequestInputSearchSettlementAdvancePopupComponent, { static: false }) inputSearchSettlementAdvancePopup: UnlockRequestInputSearchSettlementAdvancePopupComponent;
    @Input() unlockType: string;

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
        console.log(this.unlockType);
    }

    addDetail() {
        console.log(this.unlockType);
        if (this.unlockType === CommonEnum.unlockTypeEnum.ADVANCE || this.unlockType === CommonEnum.unlockTypeEnum.SETTEMENT) {
            this.inputSearchSettlementAdvancePopup.unlockType = this.unlockType;
            this.inputSearchSettlementAdvancePopup.show();
        }
        if (this.unlockType === CommonEnum.unlockTypeEnum.SHIPMENT || this.unlockType === CommonEnum.unlockTypeEnum.SERVICEDATE) {
            this.inputSearchJobPopup.show();
        }
    }
}