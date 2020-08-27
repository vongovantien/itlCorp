import { Component, OnInit } from "@angular/core";
import { Store } from "@ngrx/store";
import { AppForm } from "src/app/app.form";
import { takeUntil } from "rxjs/operators";
import { HouseBill } from "@models";
import { IShareBussinessState, getDetailHBlState } from "../../store";

@Component({
    selector: 'attach-list-house-bill',
    templateUrl: './attach-list-house-bill.component.html'
})
export class ShareBusinessAttachListHouseBillComponent extends AppForm implements OnInit {

    attachList: string = '';

    constructor(
        private _store: Store<IShareBussinessState>
    ) {
        super();
    }

    ngOnInit(): void {
        this._store.select(getDetailHBlState)
            .pipe(takeUntil(this.ngUnsubscribe))
            .subscribe(
                (hbl: HouseBill) => {
                    if (!!hbl && !!hbl.id) {
                        this.attachList = hbl.attachList;
                    }
                }
            );
    }
}