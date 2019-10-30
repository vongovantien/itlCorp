import { Component, OnInit } from '@angular/core';
import { AppList } from 'src/app/app.list';
import { CsTransactionDetail } from 'src/app/shared/models/document/csTransactionDetail';
import { DocumentationRepo } from 'src/app/shared/repositories';
import { SortService } from 'src/app/shared/services';
import { Router } from '@angular/router';
@Component({
    selector: 'app-house-bill-list',
    templateUrl: './house-bill-list.component.html',
    styleUrls: ['./house-bill-list.component.scss']
})
export class HouseBillListComponent extends AppList {
    headers: CommonInterface.IHeaderTable[];
    houseBill: CsTransactionDetail[] = [];

    constructor(
        private _sortService: SortService,
        private _router: Router,

        private _documentRepo: DocumentationRepo
    ) {
        super();
        this.requestSort = this.sortLocal;

    }

    ngOnInit() {
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
    getHourseBill() {
        this._documentRepo.getListHourseBill({}).subscribe(
            (res: any) => {

                this.houseBill = res;
                console.log(this.houseBill);
            },
        );
    }

    sortLocal(sort: string): void {
        this.houseBill = this._sortService.sort(this.houseBill, sort, this.order);
    }

    gotoCreateHouseBill() {
        this._router.navigate(['/home/documentation/sea-fcl-import/new-house-bill']);
    }


}
