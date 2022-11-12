import { Component, OnInit } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { Store } from '@ngrx/store';
import { ToastrService } from 'ngx-toastr';
import { CatalogueRepo, DocumentationRepo, ExportRepo, SystemFileManageRepo } from '@repositories';
import { SortService } from '@services';
import { AppShareHBLBase, IShareBussinessState } from '@share-bussiness';
import { NgxSpinnerService } from 'ngx-spinner';

import { takeUntil } from 'rxjs/operators';
import { merge } from 'rxjs';

@Component({
    selector: 'app-air-export-hbl',
    templateUrl: './air-export-hbl.component.html'
})

export class AirExportHBLComponent extends AppShareHBLBase implements OnInit {

    serviceType: CommonType.SERVICE_TYPE = 'air';

    constructor(
        protected _router: Router,
        protected _store: Store<IShareBussinessState>,
        protected _documentRepo: DocumentationRepo,
        protected _toastService: ToastrService,
        protected _spinner: NgxSpinnerService,
        protected _sortService: SortService,
        protected _activedRoute: ActivatedRoute,
        protected _catalogueRepo: CatalogueRepo,
        protected _exportRepo: ExportRepo,
        protected _fileMngtRepo: SystemFileManageRepo

    ) {
        super(_sortService, _store, _spinner, _toastService, _documentRepo, _activedRoute, _router, _catalogueRepo, _exportRepo, _fileMngtRepo);
    }

    listenShortcutMovingTab() {
        merge(this.utility.createShortcut(['ControlLeft', 'ShiftLeft', 'Digit1'])).pipe(takeUntil(this.ngUnsubscribe)).subscribe(() => { this.onSelectTab('shipment'); });
        merge(this.utility.createShortcut(['ControlLeft', 'ShiftLeft', 'Digit2'])).pipe(takeUntil(this.ngUnsubscribe)).subscribe(() => { this.onSelectTab('hbl'); });
        merge(this.utility.createShortcut(['ControlLeft', 'ShiftLeft', 'Digit3'])).pipe(takeUntil(this.ngUnsubscribe)).subscribe(() => { this.onSelectTab('cdNote'); });
        merge(this.utility.createShortcut(['ControlLeft', 'ShiftLeft', 'Digit4'])).pipe(takeUntil(this.ngUnsubscribe)).subscribe(() => { this.onSelectTab('assignment'); });
        merge(this.utility.createShortcut(['ControlLeft', 'ShiftLeft', 'Digit5'])).pipe(takeUntil(this.ngUnsubscribe)).subscribe(() => { this.onSelectTab('files'); });
    }
}
