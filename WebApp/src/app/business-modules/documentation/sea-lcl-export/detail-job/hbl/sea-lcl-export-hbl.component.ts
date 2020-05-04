import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { Store } from '@ngrx/store';
import { ToastrService } from 'ngx-toastr';
import { NgProgress } from '@ngx-progressbar/core';
import { DocumentationRepo } from '@repositories';
import { SortService } from '@services';
import { NgxSpinnerService } from 'ngx-spinner';
import { AppShareHBLBase, IShareBussinessState } from '@share-bussiness';

import { catchError, finalize } from 'rxjs/operators';

@Component({
    selector: 'app-sea-lcl-export-hbl',
    templateUrl: './sea-lcl-export-hbl.component.html'
})

export class SeaLCLExportHBLComponent extends AppShareHBLBase implements OnInit {
    constructor(
        private _router: Router,
        protected _store: Store<IShareBussinessState>,
        protected _documentRepo: DocumentationRepo,
        protected _toastService: ToastrService,
        protected _progressService: NgProgress,
        protected _sortService: SortService,
        protected _spinner: NgxSpinnerService

    ) {
        super(_sortService, _store, _spinner, _progressService, _toastService, _documentRepo);
    }

    configHBL() {
        this.headers = [
            { title: 'HBL No', field: 'hwbno', sortable: true, width: 100 },
            { title: 'Customer', field: 'customerName', sortable: true },
            { title: 'Salesman', field: 'saleManName', sortable: true },
            { title: 'Departure', field: 'finalDestinationPlace', sortable: true },
            { title: 'Destination', field: 'finalDestinationPlace', sortable: true },
            { title: 'Package', field: 'packages', sortable: true },
            { title: 'G.W', field: 'gw', sortable: true },
            { title: 'CBM', field: 'cbm', sortable: true }
        ];
    }

    gotoDetail(id: string) {
        this._documentRepo.checkDetailShippmentPermission(this.shipmentDetail.id)
            .pipe(
                catchError(this.catchError),
                finalize(() => this._progressRef.complete())
            ).subscribe(
                (res: any) => {
                    if (res) {
                        this._router.navigate([`/home/documentation/sea-lcl-export/${this.jobId}/hbl/${id}`]);
                    } else {
                        this.info403Popup.show();
                    }
                },
            );
    }

    gotoList() {
        this._router.navigate(["home/documentation/sea-lcl-export"]);
    }

    gotoCreate() {
        this._router.navigate([`/home/documentation/sea-lcl-export/${this.jobId}/hbl/new`]);
    }

    onSelectTab(tabName: string) {
        switch (tabName) {
            case 'shipment':
                this._router.navigate([`home/documentation/sea-lcl-export/${this.jobId}`], { queryParams: { tab: 'SHIPMENT' } });
                break;
            case 'cdNote':
                this._router.navigate([`home/documentation/sea-lcl-export/${this.jobId}`], { queryParams: { tab: 'CDNOTE' } });
                break;
            case 'assignment':
                this._router.navigate([`home/documentation/sea-lcl-export/${this.jobId}`], { queryParams: { tab: 'ASSIGNMENT' } });
                break;
        }
    }

    duplicateConfirm() {
        this._router.navigate([`home/documentation/sea-lcl-export/${this.jobId}`], {
            queryParams: Object.assign({}, { tab: 'SHIPMENT' }, { action: 'copy' })
        });
    }
}
