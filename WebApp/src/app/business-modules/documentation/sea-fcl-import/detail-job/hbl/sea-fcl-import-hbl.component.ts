import { Component } from '@angular/core';
import { AppList } from 'src/app/app.list';
import { Router, ActivatedRoute, Params } from '@angular/router';
import { CsTransactionDetail } from 'src/app/shared/models/document/csTransactionDetail';
import { DocumentationRepo } from 'src/app/shared/repositories';
import { SortService } from 'src/app/shared/services';

@Component({
    selector: 'app-sea-fcl-import-hbl',
    templateUrl: './sea-fcl-import-hbl.component.html',
})
export class SeaFCLImportHBLComponent extends AppList {
    jobId: string = '';
    headers: CommonInterface.IHeaderTable[];
    houseBill: CsTransactionDetail[] = [];
    constructor(
        private _router: Router,
        private _sortService: SortService,
        private _documentRepo: DocumentationRepo,
        private _activedRoute: ActivatedRoute
    ) {
        super();
        this.requestSort = this.sortLocal;
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
        }
    }

    sortLocal(sort: string): void {
        this.houseBill = this._sortService.sort(this.houseBill, sort, this.order);
    }

    gotoCreateHouseBill() {
        this._router.navigate([`/home/documentation/sea-fcl-import/${this.jobId}/hbl/new`]);
    }


    getHourseBill() {
        this._documentRepo.getListHourseBill({}).subscribe(
            (res: any) => {

                this.houseBill = res;
                console.log(this.houseBill);
            },
        );
    }
}
