import { Component, OnInit, Output, EventEmitter } from '@angular/core';
import { Store } from '@ngrx/store';
import { DIM } from '@models';
import { ToastrService } from 'ngx-toastr';

import { PopupBase } from 'src/app/popup.base';
import { SystemConstants } from 'src/constants/system.const';

import { skip, takeUntil } from 'rxjs/operators';

import * as fromStore from './../../store';

@Component({
    selector: 'dim-volume-popup',
    templateUrl: './dim-volume.popup.html'
})

export class ShareBusinessDIMVolumePopupComponent extends PopupBase implements OnInit {

    @Output() onUpdate: EventEmitter<DIM[]> = new EventEmitter<DIM[]>();

    dims: DIM[] = [];

    hwConstant: number = SystemConstants.HW_AIR_CONSTANT; // ? 6000
    cbmConstant: number = SystemConstants.CBM_AIR_CONSTANT; // ? 166.67

    totalHW: number = null;
    totalCBM: number = null;

    isCBMChecked: boolean = false;

    constructor(
        private _toastService: ToastrService,
        private _store: Store<fromStore.IShareBussinessState>
    ) {
        super();
    }

    ngOnInit() {
        this._store.select<any>(fromStore.getDimensionVolumesState)
            .pipe(
                skip(1),
                takeUntil(this.ngUnsubscribe)
            )
            .subscribe(
                (dims: DIM[]) => {
                    console.log("dims from store", dims);
                    this.dims = dims;
                    this.calculateHWDimension();
                }
            );
    }

    addDIM() {
        this.isSubmitted = false;
        this.dims.push(new DIM());
    }

    delete(index: number) {
        this.isSubmitted = false;
        this.dims.splice(index, 1);

        this.calculateHWDimension();
    }

    saveDIM() {
        if (!this.dims.length) {
            this._toastService.warning("Please Add Dimension Information");
            return;
        }

        this.isSubmitted = true;

        if (!this.checkValidate()) {
            return;
        } else {

            console.log(this.dims);
            this.onUpdate.emit(this.dims);
            this.hide();
            this.isSubmitted = false;
        }
    }

    updateHeightWeight(dimItem: DIM) {
        // dimItem.hw = +((dimItem.length * dimItem.height * dimItem.width / this.hwContstant) * dimItem.package).toFixed(3);
        // dimItem.cbm = +((dimItem.length * dimItem.height * dimItem.width / this.hwContstant / this.cbmConstant) * dimItem.package).toFixed(3);
        dimItem.hw = this.utility.calculateHeightWeight(dimItem.width, dimItem.height, dimItem.length, dimItem.package, this.hwConstant);
        dimItem.cbm = this.utility.calculateCBM(dimItem.width, dimItem.height, dimItem.length, dimItem.package, this.hwConstant);

        this.updateTotalHeightWeight();
        this.updateCBM();
    }

    calculateHWDimension() {
        if (!!this.dims.length) {
            for (const item of this.dims) {
                this.updateHeightWeight(item);
            }
        }
    }

    updateTotalHeightWeight() {
        this.totalHW = +this.dims.reduce((acc: number, curr: DIM) => acc += curr.hw, 0).toFixed(3);
    }

    updateCBM() {
        this.totalCBM = +this.dims.reduce((acc: number, curr: DIM) => acc += curr.cbm, 0).toFixed(3);
    }

    checkValidate() {
        let valid: boolean = true;
        for (const item of this.dims) {
            if (
                item.length === null
                || item.width === null
                || item.height === null
                || item.package === null
            ) {
                valid = false;
                break;
            }
        }
        return valid;
    }
}
