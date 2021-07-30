<?php
/**
 * Plugin Name: Phylab Research
 * Plugin URI: https://phylab.entertain.univie.ac.at/
 * Description: Research Plugin for the Phylab Project
 * Version: 1.0
 * Author: Patrick David Pazour
 * Author URI: https://pazour.at
 */

add_action( 'init', 'log_phylab_activity' );
function log_phylab_activity() {
    register_post_type( 'phylab_activities',
        array(
            'labels' => array(
                'name' => 'Phylab Activities',
                'singular_name' => 'Phylab Activity',
                'add_new' => 'Add New',
                'add_new_item' => 'Add New Phylab Activity',
                'edit' => 'Edit',
                'edit_item' => 'Edit Phylab Activity',
                'new_item' => 'New Phylab Activity',
                'view' => 'View',
                'view_item' => 'View Phylab Activity',
                'search_items' => 'Search Phylab Activities',
                'not_found' => 'No Phylab Activities found',
                'not_found_in_trash' => 'No Phylab Activities found in Trash',
                'parent' => 'Parent Phylab Activity'
            ),

            'show_in_rest' => true,
            'rest_controller_class' => 'WP_REST_Posts_Controller',
            'rest_base' => 'phylab_activities',
            'show_in_nav_menus' => true,
            'show_ui' => true,
            'show_in_menu' => true,
            'show_in_admin_bar' => true,
            'exclude_from_search' => false,
            //'menu_position' => 15,
            //'supports' => array( 'title', 'editor', 'comments', 'thumbnail', 'custom-fields' ),
            'supports' => array( '' ),
            //'taxonomies' => array( '' ),
            //'menu_icon' => plugins_url( 'images/image.png', __FILE__ ),
            'has_archive' => false
        )
    );
}

add_action( 'admin_init', 'my_admin' );
function my_admin() {
    add_meta_box( 'phylab_activity_meta_box',
        'Phylab Activity Details',
        'display_phylab_activity_meta_box',
        'phylab_activities', 'normal', 'high'
    );
}

function display_phylab_activity_meta_box( $phylab_activity ) {
    $phylab_timestamp = intval( get_post_meta( $phylab_activity->ID, 'phylab_timestamp', true ) );
    $phylab_event_name = get_post_meta( $phylab_activity->ID, 'phylab_event_name', true );
    $phylab_event_description = get_post_meta( $phylab_activity->ID, 'phylab_event_description', true );
    $phylab_usercode = get_post_meta( $phylab_activity->ID, 'phylab_usercode', true );
    ?>
    <table>
        <tr>
            <td style="width: 100%">Timestamp</td>
            <td><input type="number" step="1" size="10" name="phylab_timestamp" value="<?php echo $phylab_timestamp; ?>" /></td>
        </tr>
        <tr>
            <td style="width: 150px">Usercode</td>
            <td><input type="text" size="20" name="phylab_usercode" value="<?php echo $phylab_usercode; ?>" /></td>
        </tr>
        <tr>
            <td style="width: 150px">Event Name</td>
            <td><input type="text" size="80" name="phylab_event_name" value="<?php echo $phylab_event_name; ?>" /></td>
        </tr>
        <tr>
            <td style="width: 150px">Event Description</td>
            <td><input type="text" size="80" name="phylab_event_description" value="<?php echo $phylab_event_description; ?>" /></td>
        </tr>
    </table>
    <?php
}

add_action( 'save_post', 'add_phylab_activity_fields', 10, 2 );
function add_phylab_activity_fields( $phylab_activity_id, $phylab_activity ) {
    if ( $phylab_activity->post_type == 'phylab_activities' ) {
        if ( isset( $_POST['phylab_timestamp'] ) && $_POST['phylab_timestamp'] != '' ) {
            update_post_meta( $phylab_activity_id, 'phylab_timestamp', $_POST['phylab_timestamp'] );
        }
        if ( isset( $_POST['phylab_usercode'] ) && $_POST['phylab_usercode'] != '' ) {
            update_post_meta( $phylab_activity_id, 'phylab_usercode', $_POST['phylab_usercode'] );
        }
        if ( isset( $_POST['phylab_event_name'] ) && $_POST['phylab_event_name'] != '' ) {
            update_post_meta( $phylab_activity_id, 'phylab_event_name', $_POST['phylab_event_name'] );
        }
        if ( isset( $_POST['phylab_event_description'] ) && $_POST['phylab_event_description'] != '' ) {
            update_post_meta( $phylab_activity_id, 'phylab_event_description', $_POST['phylab_event_description'] );
        }
        if ( $phylab_activity->post_status != 'private' ) {
            wp_update_post( array( 'ID' => $phylab_activity_id, 'post_status' => 'private' ) );
        }
    }
}

add_filter( 'manage_edit-phylab_activities_columns', 'my_columns' );
function my_columns( $columns ) {
    $columns['phylab_activities_timestamp'] = 'Timestamp';
    $columns['phylab_activities_usercode'] = 'Usercode';
    $columns['phylab_activities_event_name'] = 'Event Name';
    $columns['phylab_activities_event_description'] = 'Event Description';
    $columns['phylab_activities_edit'] = 'Edit';
    unset( $columns['title'] );
    unset( $columns['date'] );
    return $columns;
}

add_action( 'manage_posts_custom_column', 'populate_columns' );
function populate_columns( $column ) {
    if ( 'phylab_activities_timestamp' == $column ) {
        $phylab_timestamp = intval( get_post_meta( get_the_ID(), 'phylab_timestamp', true ) );
        echo $phylab_timestamp;
    }
    elseif ( 'phylab_activities_usercode' == $column ) {
        $phylab_usercode = get_post_meta( get_the_ID(), 'phylab_usercode', true );
        echo $phylab_usercode;
    }
    elseif ( 'phylab_activities_event_name' == $column ) {
        $phylab_event_name = get_post_meta( get_the_ID(), 'phylab_event_name', true );
        echo $phylab_event_name;
    }
    elseif ( 'phylab_activities_event_description' == $column ) {
        $phylab_event_description = get_post_meta( get_the_ID(), 'phylab_event_description', true );
        echo $phylab_event_description;
    }
    elseif ( 'phylab_activities_edit' == $column ) {
        echo '<a href="' . get_edit_post_link( get_the_ID() ) . '">EDIT</a>';
    }
}

add_filter( 'manage_edit-phylab_activities_sortable_columns', 'sort_me' );
function sort_me( $columns ) {
    $columns['phylab_activities_timestamp'] = 'phylab_activities_timestamp';
    $columns['phylab_activities_usercode'] = 'phylab_activities_usercode';
    $columns['phylab_activities_event_name'] = 'phylab_activities_event_name';

    return $columns;
}

add_filter( 'request', 'column_orderby' );

function column_orderby ( $vars ) {
    if ( !is_admin() )
        return $vars;
    if ( isset( $vars['orderby'] ) && 'phylab_activities_usercode' == $vars['orderby'] ) {
        $vars = array_merge( $vars, array( 'meta_key' => 'phylab_usercode', 'orderby' => 'meta_value' ) );
    }
    elseif ( isset( $vars['orderby'] ) && 'phylab_activities_timestamp' == $vars['orderby'] ) {
        $vars = array_merge( $vars, array( 'meta_key' => 'phylab_timestamp', 'orderby' => 'meta_value_num' ) );
    }
    elseif ( isset( $vars['orderby'] ) && 'phylab_activities_event_name' == $vars['orderby'] ) {
        $vars = array_merge( $vars, array( 'meta_key' => 'phylab_event_name', 'orderby' => 'meta_value' ) );
    }
    return $vars;
}

//REST
add_action('rest_api_init','phylab_register_rest_fields');
function phylab_register_rest_fields(){
    register_rest_field('phylab_activities',
        'usercode',
        array(
            'get_callback'    => 'phylab_get_usercode',
            'update_callback' => 'phylab_update_usercode',
            'schema'          => null
        )
    );
    register_rest_field('phylab_activities',
        'timestamp',
        array(
            'get_callback'    => 'phylab_get_timestamp',
            'update_callback' => 'phylab_update_timestamp',
            'schema'          => null
        )
    );
    register_rest_field('phylab_activities',
        'event_name',
        array(
            'get_callback'    => 'phylab_get_event_name',
            'update_callback' => 'phylab_update_event_name',
            'schema'          => null
        )
    );
    register_rest_field('phylab_activities',
        'event_description',
        array(
            'get_callback'    => 'phylab_get_event_description',
            'update_callback' => 'phylab_update_event_description',
            'schema'          => null
        )
    );
}

function phylab_get_usercode( $post, $field_name, $request ) {
    return get_post_meta( $post['id'], 'phylab_usercode', true );
}

function phylab_update_usercode( $value, $post, $field_name ) {
    update_post_meta( $post->ID, 'phylab_usercode', $value );
}

function phylab_get_timestamp( $post, $field_name, $request ) {
    return get_post_meta( $post['id'], 'phylab_timestamp', true );
}

function phylab_update_timestamp( $value, $post, $field_name ) {
    update_post_meta( $post->ID, 'phylab_timestamp', $value );
}

function phylab_get_event_name( $post, $field_name, $request ) {
    return get_post_meta( $post['id'], 'phylab_event_name', true );
}

function phylab_update_event_name( $value, $post, $field_name ) {
    update_post_meta( $post->ID, 'phylab_event_name', $value );
}

function phylab_get_event_description( $post, $field_name, $request ) {
    return get_post_meta( $post['id'], 'phylab_event_description', true );
}

function phylab_update_event_description( $value, $post, $field_name ) {
    update_post_meta( $post->ID, 'phylab_event_description', $value );
}

?>