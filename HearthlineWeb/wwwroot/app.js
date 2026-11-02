const state = {
  dashboard: null,
  residents: [],
  apartments: [],
  maintenance: [],
  guests: [],
  announcements: [],
  events: [],
};

const elements = {
  statsGrid: document.getElementById('statsGrid'),
  announcementList: document.getElementById('announcementList'),
  eventList: document.getElementById('eventList'),
  residentsTable: document.getElementById('residentsTable'),
  apartmentTable: document.getElementById('apartmentTable'),
  maintenanceTable: document.getElementById('maintenanceTable'),
  guestTable: document.getElementById('guestTable'),
  announcementDetail: document.getElementById('announcementDetail'),
  eventDetail: document.getElementById('eventDetail'),
  pageTitle: document.getElementById('pageTitle'),
  navButtons: [...document.querySelectorAll('.nav-item')],
  tabPanels: [...document.querySelectorAll('.tab-panel')],
};

async function loadData() {
  const [dashboard, residents, apartments, maintenance, guests, announcements, events] = await Promise.all([
    fetch('/api/dashboard').then(r => r.json()),
    fetch('/api/residents').then(r => r.json()),
    fetch('/api/apartments').then(r => r.json()),
    fetch('/api/maintenance').then(r => r.json()),
    fetch('/api/guests').then(r => r.json()),
    fetch('/api/announcements').then(r => r.json()),
    fetch('/api/events').then(r => r.json()),
  ]);

  state.dashboard = dashboard;
  state.residents = residents;
  state.apartments = apartments;
  state.maintenance = maintenance;
  state.guests = guests;
  state.announcements = announcements;
  state.events = events;

  renderDashboard();
  renderResidents();
  renderApartments();
  renderMaintenance();
  renderGuests();
  renderAnnouncements();
  renderEvents();
}

function renderDashboard() {
  const { residents, apartments, openMaintenance, events, announcements, activeGuests } = state.dashboard;

  const stats = [
    { label: 'Residents', value: residents, detail: 'Living in the building' },
    { label: 'Apartments', value: apartments, detail: 'Units tracked' },
    { label: 'Open issues', value: openMaintenance, detail: 'Needs attention' },
    { label: 'Events', value: events, detail: 'Scheduled activities' },
    { label: 'Announcements', value: announcements, detail: 'Published notices' },
    { label: 'Guest passes', value: activeGuests, detail: 'Pending or approved' },
  ];

  elements.statsGrid.innerHTML = stats.map(item => `
    <div class="stat-card">
      <div class="stat-label">${item.label}</div>
      <div class="stat-value">${item.value}</div>
      <div class="stat-detail">${item.detail}</div>
    </div>
  `).join('');

  elements.announcementList.innerHTML = state.announcements.slice(0, 3).map(item => `
    <div class="list-item">
      <h4>${item.headline}</h4>
      <p>${item.message}</p>
    </div>
  `).join('');

  elements.eventList.innerHTML = state.events.slice(0, 3).map(item => `
    <div class="event-item">
      <h4>${item.name}</h4>
      <p>${item.when} • ${item.location}</p>
    </div>
  `).join('');
}

function renderResidents() {
  elements.residentsTable.innerHTML = state.residents.map(item => `
    <tr><td>${item.id}</td><td>${item.name}</td><td>${item.apartment}</td><td>${item.email}</td><td><span class="badge">${item.status}</span></td></tr>
  `).join('');
}

function renderApartments() {
  elements.apartmentTable.innerHTML = state.apartments.map(item => `
    <tr><td>${item.number}</td><td>${item.resident}</td><td>${item.shower}</td><td>${item.airConditioning}</td><td>${item.heating}</td><td>${item.appliances}</td></tr>
  `).join('');
}

function renderMaintenance() {
  elements.maintenanceTable.innerHTML = state.maintenance.map(item => `
    <tr><td>${item.id}</td><td>${item.apartment}</td><td>${item.issue}</td><td>${item.category}</td><td>${item.priority}</td><td><span class="badge">${item.status}</span></td></tr>
  `).join('');
}

function renderGuests() {
  elements.guestTable.innerHTML = state.guests.map(item => `
    <tr><td>${item.guest}</td><td>${item.host}</td><td>${item.apartment}</td><td>${item.arrival}</td><td><span class="badge">${item.passStatus}</span></td></tr>
  `).join('');
}

function renderAnnouncements() {
  elements.announcementDetail.innerHTML = state.announcements.map(item => `
    <div class="list-item">
      <h4>${item.headline}</h4>
      <p><strong>Audience:</strong> ${item.audience} • <strong>Published:</strong> ${item.published}</p>
      <p>${item.message}</p>
    </div>
  `).join('');
}

function renderEvents() {
  elements.eventDetail.innerHTML = state.events.map(item => `
    <div class="event-item">
      <h4>${item.name}</h4>
      <p><strong>When:</strong> ${item.when}</p>
      <p><strong>Location:</strong> ${item.location}</p>
      <p><strong>Attendance:</strong> ${item.attendance}</p>
    </div>
  `).join('');
}

function setTab(tabName) {
  elements.navButtons.forEach(button => button.classList.toggle('active', button.dataset.tab === tabName));
  elements.tabPanels.forEach(panel => panel.classList.toggle('active', panel.id === tabName));

  const titleMap = {
    dashboard: 'Overview',
    residents: 'Residents',
    apartments: 'Apartments',
    maintenance: 'Maintenance',
    guests: 'Guests',
    announcements: 'Announcements',
    events: 'Events',
  };

  elements.pageTitle.textContent = titleMap[tabName] || 'Overview';
}

elements.navButtons.forEach(button => {
  button.addEventListener('click', () => setTab(button.dataset.tab));
});

loadData().catch(error => {
  console.error('Failed to load app data', error);
  elements.statsGrid.innerHTML = '<div class="stat-card"><div class="stat-label">Status</div><div class="stat-value">Offline</div><div class="stat-detail">The demo service is unavailable.</div></div>';
});
