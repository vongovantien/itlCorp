import { NgModule } from "@angular/core";
import { LAZY_MODULES_MAP, ILazyModules } from "src/app/load-module-map";

export const lazyModulesFClMap: ILazyModules = {
    SeaLCLExport_cdNote: () => import('./detail-job/cd-note/sea-lcl-export-cd-note.module').then(m => m.SeaLCLExportCDNoteModule),
    SeaLCLExport_assignment: () => import('./detail-job/assignment/sea-lcl-export-assignment.module').then(m => m.SeaLCLExportAsignmentModule),
    SeaLCLExport_advanceSettle: () => import('./detail-job/advance-settle/sea-lcl-export-advance-settle.module').then(m => m.SeaLCLExportAdvanceSettleModule)
};

@NgModule({
    providers: [
        {
            provide: LAZY_MODULES_MAP,
            useFactory: () => lazyModulesFClMap
        }
    ]
})
export class SeaLCLExportLazyLoadModule { }
