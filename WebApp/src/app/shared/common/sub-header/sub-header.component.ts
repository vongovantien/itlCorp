import { Component, ViewChild } from '@angular/core';
import { BreadcrumbComponent } from '../breadcrumb/breadcrumb.component';

@Component({
    selector: 'app-sub-header',
    templateUrl: './sub-header.component.html'
})

export class SubHeaderComponent {
    @ViewChild(BreadcrumbComponent, { static: false }) breadcrumComponent: BreadcrumbComponent;
    // activeRoute: any[];
    constructor() {
    }

    ngOnInit() { }

    resetBreadcrumb(name: string) {

        const storagedRoutes = localStorage.getItem("ActiveRoute");
        const activeRoute = storagedRoutes == null ? [] : JSON.parse(storagedRoutes);
        const level = activeRoute[activeRoute.length - 1].level;
        const index = activeRoute.findIndex(x => x.level === level);
        if (index > -1) {
            activeRoute[index].name = name;
        }
        this.breadcrumComponent.ActiveRoute = activeRoute;
        console.log(index);
    }
}