import { Component, Output, EventEmitter, Input } from "@angular/core";
import { PopupBase } from "src/app/popup.base";
import { CommonEnum } from "@enums";

@Component({
    selector: 'input-search-settlement-advance-popup',
    templateUrl: './input-search-settlement-advance.popup.html'
})
export class UnlockRequestInputSearchSettlementAdvancePopupComponent extends PopupBase {
    @Output() onInputJob: EventEmitter<any> = new EventEmitter<any>();
    @Input() unlockType: string = CommonEnum.unlockTypeEnum.SETTEMENT;
    settlementAdvanceSearch: string = '';
    constructor(
    ) {
        super();
    }

    ngOnInit() { }

    add() {

    }

    closePopup() {
        this.hide();
    }
}