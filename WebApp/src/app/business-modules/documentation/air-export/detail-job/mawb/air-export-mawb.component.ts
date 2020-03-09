import { Component, OnInit } from '@angular/core';
import { AppForm } from 'src/app/app.form';
import { Store } from '@ngrx/store';
import { IAppState } from '@store';
import { getTransactionLocked, getTransactionPermission } from '@share-bussiness';

@Component({
    selector: 'app-air-export-mawb',
    templateUrl: './air-export-mawb.component.html'
})

export class AirExportMAWBFormComponent extends AppForm implements OnInit {
    constructor(
        private _store: Store<IAppState>
    ) { super(); }

    ngOnInit() {
        this.isLocked = this._store.select(getTransactionLocked);
        this.permissionShipments = this._store.select(getTransactionPermission);
    }

    onSaveMAWB() {

    }
}
