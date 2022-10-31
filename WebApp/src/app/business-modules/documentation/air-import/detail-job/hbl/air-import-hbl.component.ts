import { Component, OnInit } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { Store } from '@ngrx/store';
import { ToastrService } from 'ngx-toastr';
import { NgxSpinnerService } from 'ngx-spinner';
import { NgProgress } from '@ngx-progressbar/core';
import { CatalogueRepo, DocumentationRepo, ExportRepo, SystemFileManageRepo } from '@repositories';
import { SortService } from '@services';
import { IShareBussinessState, AppShareHBLBase } from '@share-bussiness';


@Component({
    selector: 'app-air-import-hbl',
    templateUrl: './air-import-hbl.component.html'
})

export class AirImportHBLComponent extends AppShareHBLBase implements OnInit {

    serviceType: CommonType.SERVICE_TYPE = 'air';

    constructor(
        protected _router: Router,
        protected _store: Store<IShareBussinessState>,
        protected _documentRepo: DocumentationRepo,
        protected _toastService: ToastrService,
        protected _progressService: NgProgress,
        protected _spinner: NgxSpinnerService,
        protected _sortService: SortService,
        protected _activedRoute: ActivatedRoute,
        protected _catalogueRepo: CatalogueRepo,
        protected _exportRepo: ExportRepo,
        protected _fileMngtRepo: SystemFileManageRepo


    ) {
        super(_sortService, _store, _spinner, _toastService, _documentRepo, _activedRoute, _router, _catalogueRepo, _exportRepo, _fileMngtRepo);
    }

}
