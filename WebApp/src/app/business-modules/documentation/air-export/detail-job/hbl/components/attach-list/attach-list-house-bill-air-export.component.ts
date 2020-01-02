import { Component, OnInit } from '@angular/core';
import { AppForm } from 'src/app/app.form';
import { IShareBussinessState, getDetailHBlState } from '@share-bussiness';
import { Store } from '@ngrx/store';
import { HouseBill } from '@models';

@Component({
    selector: 'air-export-hbl-attach-list',
    templateUrl: './attach-list-house-bill-air-export.component.html',
})
export class AirExportHBLAttachListComponent extends AppForm implements OnInit {

    attachList: string = '';

    constructor(
        private _store: Store<IShareBussinessState>
    ) {
        super();
    }

    ngOnInit(): void {
        this._store.select(getDetailHBlState)
            .subscribe(
                (hbl: HouseBill) => {
                    if (!!hbl.id) {
                        this.attachList = hbl.attachList;
                    }
                }
            );
    }
}
