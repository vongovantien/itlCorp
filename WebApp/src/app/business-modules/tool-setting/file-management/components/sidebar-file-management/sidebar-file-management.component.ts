import { AfterViewInit, Component, Input, OnInit } from '@angular/core';
import { ActivatedRoute, Event, NavigationEnd, Router } from '@angular/router';
import { RoutingConstants } from '@constants';

@Component({
    selector: 'sidebar-file-management',
    templateUrl: './sidebar-file-management.component.html'
})
export class SidebarFileManagementComponent implements OnInit {
    @Input() folderName: string;
    title: string;
    constructor(private route: ActivatedRoute, private _router: Router) {

    }
    ngOnInit() {
        console.log(this.folderName);
        if (this.folderName != null && this.folderName != undefined) {
            this.title = this.route.snapshot.data['title'] + ">" + this.folderName;
        }
        else {
            this.title = this.route.snapshot.data['title']
        }
    }

    navigateFileMngt(moduleUrl: string) {
        if (moduleUrl === 'accounting') {
            this._router.navigate([RoutingConstants.TOOL.FILE_MANAGMENT]);
        }
        else {
            this._router.navigate([RoutingConstants.TOOL.FILE_MANAGMENT + "/" + moduleUrl]);
        }
    }
}
