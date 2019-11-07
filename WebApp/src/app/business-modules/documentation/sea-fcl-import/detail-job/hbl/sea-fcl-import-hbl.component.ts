import { Component, ViewChild } from '@angular/core';
import { AppList } from 'src/app/app.list';
import { Router, ActivatedRoute, Params } from '@angular/router';
import { CsTransactionDetail } from 'src/app/shared/models/document/csTransactionDetail';
import { DocumentationRepo } from 'src/app/shared/repositories';
import { SortService } from 'src/app/shared/services';
import { ConfirmPopupComponent } from 'src/app/shared/common/popup';
import { catchError, finalize } from 'rxjs/operators';
import { ToastrService } from 'ngx-toastr';
import { NgProgress } from '@ngx-progressbar/core';

@Component({
    selector: 'app-sea-fcl-import-hbl',
    templateUrl: './sea-fcl-import-hbl.component.html',
})
export class SeaFCLImportHBLComponent extends AppList {
    @ViewChild(ConfirmPopupComponent, { static: false }) confirmDeletePopup: ConfirmPopupComponent;
    jobId: string = '';
    headers: CommonInterface.IHeaderTable[];
    houseBill: CsTransactionDetail[] = [];
    selectedHbl: CsTransactionDetail;

    constructor(
        private _router: Router,
        private _sortService: SortService,
        private _documentRepo: DocumentationRepo,
        private _activedRoute: ActivatedRoute,
        private _toastService: ToastrService,
        private _progressService: NgProgress,
    ) {
        super();
        this.requestSort = this.sortLocal;
        this._progressRef = this._progressService.ref();

    }

    ngOnInit(): void {
        this._activedRoute.params.subscribe((param: Params) => {
            if (param.id) {
                this.jobId = param.id;
            }
        });
        this.headers = [
            { title: 'HBL No', field: 'hwbno', sortable: true, width: 100 },
            { title: 'Customer', field: 'customerName', sortable: true },
            { title: 'SaleMan', field: 'saleManName', sortable: true },
            { title: 'Notify Party', field: 'notifyParty', sortable: true },
            { title: 'Destination', field: 'finalDestinationPlace', sortable: true },
            { title: 'Containers', field: 'containers', sortable: true },
            { title: 'Package', field: 'packages', sortable: true },
            { title: 'G.W', field: 'gw', sortable: true },
            { title: 'CBM', field: 'cbm', sortable: true }
        ];
        this.getHourseBill();
    }

    onSelectTab(tabName: string) {
        switch (tabName) {
            case 'shipment':
                this._router.navigate([`home/documentation/sea-fcl-import/${this.jobId}`], { queryParams: { tab: 'SHIPMENT' } });
                break;
            case 'cdNote':
                this._router.navigate([`home/documentation/sea-fcl-import/${this.jobId}`], { queryParams: { tab: 'CDNOTE' } });
                break;
            // case 'hbl':
            //     this._router.navigate([`home/documentation/sea-fcl-import/${this.jobId}`], { queryParams: { tab: 'HBL' } });
            //     break;
        }
    }

    sortLocal(sort: string): void {
        this.houseBill = this._sortService.sort(this.houseBill, sort, this.order);
    }


    gotoCreateHouseBill() {
        this._router.navigate([`/home/documentation/sea-fcl-import/${this.jobId}/hbl/new`]);
    }


    showDeletePopup(hbl: CsTransactionDetail) {
        this.confirmDeletePopup.show();
        this.selectedHbl = hbl;

    }

    deleteHbl(id: string) {
        this.isLoading = true;
        this._progressRef.start();
        this._documentRepo.deleteHbl(id)
            .pipe(
                catchError(this.catchError),
                finalize(() => { this.isLoading = false; this._progressRef.complete(); }),
            ).subscribe(
                (res: CommonInterface.IResult) => {
                    if (res.status) {
                        this._toastService.success(res.message, '');
                        this.getHourseBill();
                    } else {
                        this._toastService.error(res.message || 'Có lỗi xảy ra', '');
                    }
                },
            );
    }

    onDeleteHbl() {
        this.confirmDeletePopup.hide();
        this.deleteHbl(this.selectedHbl.id);
    }


    getHourseBill() {
        this.isLoading = true;
        this._documentRepo.getListHourseBill({}).pipe(
            catchError(this.catchError),
            finalize(() => { this.isLoading = false; }),
        ).subscribe(
            (res: any) => {

                this.houseBill = res;
                console.log(this.houseBill);
            },
        );
    }
}
