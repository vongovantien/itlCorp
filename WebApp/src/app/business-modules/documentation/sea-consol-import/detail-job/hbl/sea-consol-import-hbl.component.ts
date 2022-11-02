import { Component, ViewChild } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { Store } from '@ngrx/store';
import { ToastrService } from 'ngx-toastr';
import { NgProgress } from '@ngx-progressbar/core';
import { CatalogueRepo, DocumentationRepo, ExportRepo, SystemFileManageRepo } from '@repositories';
import { SortService } from '@services';
import { NgxSpinnerService } from 'ngx-spinner';
import { AppShareHBLBase, IShareBussinessState } from '@share-bussiness';

@Component({
    selector: 'app-sea-consol-import-hbl',
    templateUrl: './sea-consol-import-hbl.component.html',
})
export class SeaConsolImportHBLComponent extends AppShareHBLBase {
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

    configHBL() {
        this.headers = [
            { title: 'HBL No', field: 'hwbno', sortable: true, width: 100 },
            { title: 'Customer', field: 'customerName', sortable: true },
            { title: 'Salesman', field: 'saleManName', sortable: true },
            { title: 'Departure', field: 'finalDestinationPlace', sortable: true },
            { title: 'Destination', field: 'finalDestinationPlace', sortable: true },
            { title: 'Container', 'field': 'containers', sortable: true },
            { title: 'Package', field: 'packages', sortable: true },
            { title: 'G.W', field: 'gw', sortable: true },
            { title: 'CBM', field: 'cbm', sortable: true },
            { title: 'Group', field: 'group', sortable: true },
            { title: 'Department', field: 'department', sortable: true }
        ];
    }

}
